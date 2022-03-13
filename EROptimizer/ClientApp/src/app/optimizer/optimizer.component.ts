import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';

import { DataService } from '../../service/data.service'
import { ArmorDataDto, IArmorDataDto } from '../../service/dto/IArmorDataDto';
import { ErrorDialogComponent } from '../error-dialog/error-dialog.component';
import { IOptimizerWorkerRS, OptimizerWorkerRSEnum } from './model/OptimizerWorkerRS';

import { OptimizerConfigDto } from './model/OptimizerConfigDto';
import { OptimizerWorkerRQ } from './model/OptimizerWorkerRQ';
import { ArmorCombo } from './model/ArmorCombo';
import { IArmorPieceDto } from '../../service/dto/IArmorPieceDto';

@Component({
  selector: 'app-optimizer',
  templateUrl: './optimizer.component.html',
  styleUrls: ['./optimizer.component.css'],
})
export class OptimizerComponent implements OnInit {

  isLoading: boolean = true;
  hasProgressBar: boolean = false;
  progressValue: number = 0;

  armorData!: IArmorDataDto;

  viewModel: OptimizerConfigDto;

  workers!: Worker[];
  progressDictionary: { [key: number]: { progress: number, status: OptimizerWorkerRSEnum } } = {};

  results: ArmorCombo[] = [];

  constructor(private dataService: DataService, private dialog: MatDialog) {
    this.viewModel = dataService.config;
  }

  ngOnInit(): void {

    this.dataService.armorData.subscribe((data: IArmorDataDto) => {
      this.armorData = data;

      if (typeof Worker === 'undefined') {
        this.dialog.open(ErrorDialogComponent, {
          data: {
            errorText: "Web Worker API is not supported on this browser. Please use a more modern browser",
          }
        });
      } else {
        this.createWorkers();
      }

      this.isLoading = false;

    }, (error: any) => {
      this.isLoading = false;
      this.dialog.open(ErrorDialogComponent, {
        data: {
          errorText: "Armor data retrieval failed.",
          errorException: error,
        }
      });
    });

  }

  createWorkers() {
    this.workers = [];
    for (let i = 0; i < this.viewModel.numberOfThreads; i++) {
      this.workers.push(this.createWorker());
    }
  }

  createWorker(): Worker {
    let worker = new Worker(new URL('./optimizer.worker', import.meta.url));
    worker.onmessage = this.handleWorkerMessageResponse.bind(this);
    return worker;
  }

  prioritizePhysicals() {
    this.viewModel.priPhysical = 1;
    this.viewModel.priPhysicalStrike = 1;
    this.viewModel.priPhysicalSlash = 1;
    this.viewModel.priPhysicalPierce = 1;

    this.viewModel.priMagic = 0;
    this.viewModel.priFire = 0;
    this.viewModel.priLightning = 0;
    this.viewModel.priHoly = 0;

    this.viewModel.priImmunity = 0;
    this.viewModel.priRobustness = 0;
    this.viewModel.priFocus = 0;
    this.viewModel.priVitality = 0;

    this.viewModel.priPoise = 0;
  }

  prioritizeElementals() {
    this.viewModel.priPhysical = 0;
    this.viewModel.priPhysicalStrike = 0;
    this.viewModel.priPhysicalSlash = 0;
    this.viewModel.priPhysicalPierce = 0;

    this.viewModel.priMagic = 1;
    this.viewModel.priFire = 1;
    this.viewModel.priLightning = 1;
    this.viewModel.priHoly = 1;

    this.viewModel.priImmunity = 0;
    this.viewModel.priRobustness = 0;
    this.viewModel.priFocus = 0;
    this.viewModel.priVitality = 0;

    this.viewModel.priPoise = 0;
  }

  prioritizeBoth() {
    this.viewModel.priPhysical = 1;
    this.viewModel.priPhysicalStrike = 1;
    this.viewModel.priPhysicalSlash = 1;
    this.viewModel.priPhysicalPierce = 1;

    this.viewModel.priMagic = 1;
    this.viewModel.priFire = 1;
    this.viewModel.priLightning = 1;
    this.viewModel.priHoly = 1;

    this.viewModel.priImmunity = 0;
    this.viewModel.priRobustness = 0;
    this.viewModel.priFocus = 0;
    this.viewModel.priVitality = 0;

    this.viewModel.priPoise = 0;
  }

  runOptimization(): void {

    this.isLoading = true;
    this.hasProgressBar = true;
    this.progressValue = 0;

    this.progressDictionary = {};
    for (let i = 0; i < this.viewModel.numberOfThreads; i++) {
      this.progressDictionary[i] = { progress: 0, status: OptimizerWorkerRSEnum.Progress };
    }

    this.results = [];

    if (this.workers.length != this.viewModel.numberOfThreads) {

      let newWorkerArray: Worker[] = [];

      let i = 0;
      while (i < this.viewModel.numberOfThreads) {
        if (i < this.workers.length)
          newWorkerArray.push(this.workers[i]);
        else
          newWorkerArray.push(this.createWorker());

        i++;
      }

      while (i < this.workers.length) {
        this.workers[i].terminate();
        i++;
      }

      this.workers = newWorkerArray;
    }
    
    let data = new ArmorDataDto({
      head: this.armorData.head.filter(x => x.isEnabled),
      chest: this.armorData.chest.filter(x => x.isEnabled),
      gauntlets: this.armorData.gauntlets.filter(x => x.isEnabled),
      legs: this.armorData.legs.filter(x => x.isEnabled)
    });

    if (data.head.length == 0) {
      this.dialog.open(ErrorDialogComponent, { data: { errorText: "All Head pieces are disabled. Please enable at least one Head piece." } });
      return;
    } else if (data.chest.length == 0) {
      this.dialog.open(ErrorDialogComponent, { data: { errorText: "All Chest pieces are disabled. Please enable at least one Chest piece." } });
      return;
    } else if (data.gauntlets.length == 0) {
      this.dialog.open(ErrorDialogComponent, { data: { errorText: "All Gauntlets pieces are disabled. Please enable at least one Gauntlets piece." } });
      return;
    } else if (data.legs.length == 0) {
      this.dialog.open(ErrorDialogComponent, { data: { errorText: "All Legs pieces are disabled. Please enable at least one Legs piece." } });
      return;
    }

    let chunks: ArmorDataDto[] = [];
    if (
      data.head.length >= data.chest.length &&
      data.head.length >= data.gauntlets.length &&
      data.head.length >= data.legs.length) {

      var i, j, temporary, chunk = Math.ceil(data.head.length / this.viewModel.numberOfThreads);
      for (i = 0, j = data.head.length; i < j; i += chunk) {
        temporary = data.head.slice(i, i + chunk);
        chunks.push(new ArmorDataDto({
          head: temporary,
          chest: data.chest,
          gauntlets: data.gauntlets,
          legs: data.legs
        }));
      }

    } else if (
      data.chest.length >= data.gauntlets.length &&
      data.chest.length >= data.legs.length) {

      var i, j, temporary, chunk = Math.ceil(data.chest.length / this.viewModel.numberOfThreads);
      for (i = 0, j = data.chest.length; i < j; i += chunk) {
        temporary = data.chest.slice(i, i + chunk);
        chunks.push(new ArmorDataDto({
          head: data.head,
          chest: temporary,
          gauntlets: data.gauntlets,
          legs: data.legs
        }));
      }

    } else if (
      data.gauntlets.length >= data.legs.length) {

      var i, j, temporary, chunk = Math.ceil(data.gauntlets.length / this.viewModel.numberOfThreads);
      for (i = 0, j = data.gauntlets.length; i < j; i += chunk) {
        temporary = data.gauntlets.slice(i, i + chunk);
        chunks.push(new ArmorDataDto({
          head: data.head,
          chest: data.chest,
          gauntlets: temporary,
          legs: data.legs
        }));
      }

    } else {

      var i, j, temporary, chunk = Math.ceil(data.legs.length / this.viewModel.numberOfThreads);
      for (i = 0, j = data.legs.length; i < j; i += chunk) {
        temporary = data.legs.slice(i, i + chunk);
        chunks.push(new ArmorDataDto({
          head: data.head,
          chest: data.chest,
          gauntlets: data.gauntlets,
          legs: temporary
        }));
      }

    }

    if (chunks.length != this.viewModel.numberOfThreads || this.workers.length != this.viewModel.numberOfThreads) {
      this.dialog.open(ErrorDialogComponent, { data: { errorText: "My math messed up." } });
      return;
    }

    for (let i = 0; i < this.viewModel.numberOfThreads; i++) {
      this.workers[i].postMessage(new OptimizerWorkerRQ(chunks[i], this.viewModel, i));
    }
  }

  disableArmorPiece(piece: IArmorPieceDto) {
    piece.isEnabled = false;
    this.runOptimization();
  }

  cancelOptimization() {
    this.workers.forEach(x => x.terminate());
    this.createWorkers();
    this.isLoading = false;
  }

  handleWorkerMessageResponse(e: MessageEvent<IOptimizerWorkerRS>) {
    let rs: IOptimizerWorkerRS = e.data;

    if (rs.type == OptimizerWorkerRSEnum.Progress) {
      
      this.progressDictionary[rs.workerIndex].progress = rs.progress;

      let currentProgress = 0;
      for (let i = 0; i < this.viewModel.numberOfThreads; i++) {
        currentProgress += this.progressDictionary[i].progress / this.viewModel.numberOfThreads;
      }

      this.progressValue = currentProgress;

    } else if (rs.type == OptimizerWorkerRSEnum.Finished) {

      this.progressDictionary[rs.workerIndex].status = OptimizerWorkerRSEnum.Finished;

      let responseResults = rs.results.map(x =>
        new ArmorCombo(
          this.armorData.head.find(p => p.armorPieceId == x.headPieceId)!,
          this.armorData.chest.find(p => p.armorPieceId == x.chestPieceId)!,
          this.armorData.gauntlets.find(p => p.armorPieceId == x.gauntletsPieceId)!,
          this.armorData.legs.find(p => p.armorPieceId == x.legsPieceId)!, this.viewModel)
      );

      //same code as optimizer.worker.ts
      for (let i = 0; i < responseResults.length; i++) {

        let combo: ArmorCombo = responseResults[i];

        if (this.results.length == 0) {
          this.results.push(combo);
        } else if (this.results.length == this.viewModel.numberOfResults && combo.score <= this.results[this.results.length - 1].score) {
          continue;
        } else {
          for (let m = 0; m < this.results.length; m++) {
            let item: ArmorCombo = this.results[m];
            if (item.score < combo.score) {
              //Push all items down, insert at m.
              this.results.splice(m, 0, combo);

              //Remove last item if necessary
              if (this.results.length > this.viewModel.numberOfResults) this.results.splice(this.results.length - 1, 1);

              break;
            }
          }
        }

      }

      let isFinished = true;
      for (let i = 0; i < this.viewModel.numberOfThreads; i++) {
        if (this.progressDictionary[i].status != OptimizerWorkerRSEnum.Finished) {
          isFinished = false;
          break;
        }
      }

      if (isFinished) {

        if (this.results.length > 0) {
          console.log(this.results[0].head);
          console.log(this.results[0].chest);
          console.log(this.results[0].gauntlets);
          console.log(this.results[0].legs);
        }

        this.isLoading = false;
      }
    }
  }
}
