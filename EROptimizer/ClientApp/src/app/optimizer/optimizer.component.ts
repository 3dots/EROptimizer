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

  worker!: Worker;

  results: ArmorCombo[] = [];

  constructor(private dataService: DataService, private dialog: MatDialog) {
    this.viewModel = dataService.config;
  }

  ngOnInit(): void {

    this.dataService.armorData.subscribe((data: IArmorDataDto) => {
      this.armorData = data;
      this.isLoading = false;

      if (typeof Worker === 'undefined') {
        this.dialog.open(ErrorDialogComponent, {
          data: {
            errorText: "Web Worker API is not supported on this browser. Please use a more modern browser",
          }
        });
      }

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

    this.worker = new Worker(new URL('./optimizer.worker', import.meta.url));
    this.worker.onmessage = (e: MessageEvent<IOptimizerWorkerRS>) => {

      let rs: IOptimizerWorkerRS = e.data;

      if (rs.type == OptimizerWorkerRSEnum.Progress) {
        this.progressValue = rs.progress;
      } else if (rs.type == OptimizerWorkerRSEnum.Finished) {

        this.results = rs.results.map(x =>
          new ArmorCombo(
            this.armorData.head.find(p => p.armorPieceId == x.headPieceId)!,
            this.armorData.chest.find(p => p.armorPieceId == x.chestPieceId)!,
            this.armorData.gauntlets.find(p => p.armorPieceId == x.gauntletsPieceId)!,
            this.armorData.legs.find(p => p.armorPieceId == x.legsPieceId)!, null)
        );

        this.isLoading = false;
      }

    };

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

    this.worker.postMessage(new OptimizerWorkerRQ(data, this.viewModel));
  }

  disableArmorPiece(piece: IArmorPieceDto) {
    piece.isEnabled = false;
    this.runOptimization();
  }

}
