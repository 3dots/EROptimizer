import { AfterViewInit, Component, ElementRef, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { Observable, startWith, map } from 'rxjs';

import { DataService } from '../../service/data.service'
import { ArmorDataDto, IArmorDataDto } from '../../service/dto/IArmorDataDto';
import { ErrorDialogComponent, ErrorDialogData } from '../error-dialog/error-dialog.component';
import { IOptimizerWorkerRS, OptimizerWorkerRSEnum } from './model/OptimizerWorkerRS';

import { ConfigTypeEnum, OptimizeForEnum, OptimizerConfigDto } from './model/OptimizerConfigDto';
import { OptimizerWorkerRQ } from './model/OptimizerWorkerRQ';
import { ArmorCombo } from './model/ArmorCombo';
import { ArmorPieceTypeEnum, IArmorPieceDto } from '../../service/dto/IArmorPieceDto';
import { FormControl } from '@angular/forms';
import { ITalismanDto } from '../../service/dto/ITalismanDto';
import { MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { PrioritizationHelpComponent, PrioritizationHelpData } from '../prioritization-help/prioritization-help.component';
import { IArmorSetDto } from '../../service/dto/IArmorSetDto';
import { DialogHelper } from '../utility/dialog-helper';
import { UtilityHelper } from '../utility/utility-helper';

@Component({
  selector: 'app-optimizer',
  templateUrl: './optimizer.component.html',
  styleUrls: ['./optimizer.component.scss'],
})
export class OptimizerComponent implements OnInit, AfterViewInit {

  //#region Fields and Properties

  isLoading: boolean = true;
  hasProgressBar: boolean = false;
  progressValue: number = 0;

  armorData!: IArmorDataDto;

  viewModel: OptimizerConfigDto;

  workers!: Worker[];
  progressDictionary: { [key: number]: { progress: number, status: OptimizerWorkerRSEnum } } = {};

  results: ArmorCombo[] = [];

  numberOfDisabledPieces: number = 0;

  ConfigTypeEnum = ConfigTypeEnum;
  OptimizeForEnum = OptimizeForEnum;

  txtTalisman1: FormControl = new FormControl();
  filteredTalismans1!: Observable<ITalismanDto[]>;
  txtTalisman2: FormControl = new FormControl();
  filteredTalismans2!: Observable<ITalismanDto[]>;
  txtTalisman3: FormControl = new FormControl();
  filteredTalismans3!: Observable<ITalismanDto[]>;
  txtTalisman4: FormControl = new FormControl();
  filteredTalismans4!: Observable<ITalismanDto[]>;

  @ViewChild('optimizeForHelpDialogTemplate') optimizeForHelpDialogTemplate!: TemplateRef<any>;

  @ViewChild('mainHeading') mainHeading!: ElementRef;

  constructor(private dataService: DataService, private dialog: DialogHelper) {
    this.viewModel = dataService.model.config;
  }

  //#endregion

  //#region Events

  ngOnInit(): void {

    this.dataService.armorData.subscribe((data: IArmorDataDto) => {
      this.armorData = data;
      this.setNumberOfDisabledPieces();

      if (typeof Worker === 'undefined') {
        this.dialog.open(ErrorDialogComponent, {
          data: new ErrorDialogData({
            errorText: "Web Worker API is not supported on this browser. Please use a more modern browser",
          })
        });
      } else {
        this.createWorkers();
      }

      this.filteredTalismans1 = this.txtTalisman1.valueChanges.pipe(
        //tap(x => console.log(x)),
        startWith(""),
        map<string | ITalismanDto, string>(value => (typeof value === 'string' ? value : value.name)),
        map<string, ITalismanDto[]>(name => this.filterTalismans(name))
      );
      this.filteredTalismans2 = this.txtTalisman2.valueChanges.pipe(
        startWith(""),
        map<string | ITalismanDto, string>(value => (typeof value === 'string' ? value : value.name)),
        map<string, ITalismanDto[]>(name => this.filterTalismans(name))
      );
      this.filteredTalismans3 = this.txtTalisman3.valueChanges.pipe(
        startWith(""),
        map<string | ITalismanDto, string>(value => (typeof value === 'string' ? value : value.name)),
        map<string, ITalismanDto[]>(name => this.filterTalismans(name))
      );
      this.filteredTalismans4 = this.txtTalisman4.valueChanges.pipe(
        startWith(""),
        map<string | ITalismanDto, string>(value => (typeof value === 'string' ? value : value.name)),
        map<string, ITalismanDto[]>(name => this.filterTalismans(name))
      );

      this.bindTalismanAutocompletes();

      this.isLoading = false;      

    }, (error: any) => {
      this.isLoading = false;
      this.dialog.open(ErrorDialogComponent, {
        data: new ErrorDialogData({
          errorText: "Armor data retrieval failed.",
          errorException: error,
        })
      });
    });

  }

  ngAfterViewInit() {
    //todo: trap focus in loading element, while its up.
    //this.mainHeading.nativeElement.focus();
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

  reset() {
    this.viewModel = this.dataService.resetConfig();
    this.setNumberOfDisabledPieces();
    this.bindTalismanAutocompletes();
  }

  disableArmorPiece(piece: IArmorPieceDto) {

    let isArmorFilterOverride: boolean = false;
    switch (piece.type) {
      case ArmorPieceTypeEnum.Head: {
        isArmorFilterOverride = this.viewModel.filterOverrideHeadName == piece.name;
        if (isArmorFilterOverride) this.viewModel.filterOverrideHeadName = null;
        break;
      }
      case ArmorPieceTypeEnum.Chest: {
        isArmorFilterOverride = this.viewModel.filterOverrideChestName == piece.name;
        if (isArmorFilterOverride) this.viewModel.filterOverrideChestName = null;
        break;
      }
      case ArmorPieceTypeEnum.Gauntlets: {
        isArmorFilterOverride = this.viewModel.filterOverrideGauntletsName == piece.name;
        if (isArmorFilterOverride) this.viewModel.filterOverrideGauntletsName = null;
        break;
      }
      case ArmorPieceTypeEnum.Legs: {
        isArmorFilterOverride = this.viewModel.filterOverrideLegsName == piece.name;
        if (isArmorFilterOverride) this.viewModel.filterOverrideLegsName = null;
        break;
      }
    }

    if (!isArmorFilterOverride) {
      piece.isEnabled = false;
      this.numberOfDisabledPieces++;
    }
    
    this.runOptimization();
  }

  switchConfigType(type: ConfigTypeEnum) {
    this.viewModel.configType = type;
  }

  switchOptimizeForType(type: OptimizeForEnum) {
    this.viewModel.optimizeForType = type;
  }

  onTalismanChanged(ev: MatAutocompleteSelectedEvent) {
    this.saveTalismanIds();
  }

  clearAutocomplete(txtAutocomplete: FormControl) {
    txtAutocomplete.setValue("");
    this.saveTalismanIds();
  }

  openOptimizeForHelpDialog() {
    this.dialog.open(this.optimizeForHelpDialogTemplate, {
    });
  }

  openPrioritizationHelpDialog() {

    let set = this.armorData.armorSets.find(x => x.name == "Astrologer Set");
    if (!set) return;

    let setA: IArmorSetDto = {
      armorSetId: set.armorSetId,
      name: set.name,
      combo: new ArmorCombo(set.combo.head, set.combo.chest, set.combo.gauntlets, set.combo.legs, this.viewModel, null),
    };

    set = this.armorData.armorSets.find(x => x.name == "Bandit Set");
    if (!set) return;

    let setB: IArmorSetDto = {
      armorSetId: set.armorSetId,
      name: set.name,
      combo: new ArmorCombo(set.combo.head, set.combo.chest, set.combo.gauntlets, set.combo.legs, this.viewModel, null),
    };

    this.dialog.open(PrioritizationHelpComponent, {
      data: new PrioritizationHelpData(this.viewModel, setA, setB)
    });
  }

  //#endregion

  //#region Workers

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

  runOptimization(): void {

    //console.log(this.txtTalisman1.value);

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

    let data: ArmorDataDto = this.filterArmorData();

    if (data.head.length == 0) {
      this.dialog.open(ErrorDialogComponent, { data: new ErrorDialogData({ errorText: "All Head pieces are disabled. Please enable at least one Head piece." }) });
      this.isLoading = false;
      return;
    } else if (data.chest.length == 0) {
      this.dialog.open(ErrorDialogComponent, { data: new ErrorDialogData({ errorText: "All Chest pieces are disabled. Please enable at least one Chest piece." }) });
      this.isLoading = false;
      return;
    } else if (data.gauntlets.length == 0) {
      this.dialog.open(ErrorDialogComponent, { data: new ErrorDialogData({ errorText: "All Gauntlets pieces are disabled. Please enable at least one Gauntlets piece." }) });
      this.isLoading = false;
      return;
    } else if (data.legs.length == 0) {
      this.dialog.open(ErrorDialogComponent, { data: new ErrorDialogData({ errorText: "All Legs pieces are disabled. Please enable at least one Legs piece." }) });
      this.isLoading = false;
      return;
    }

    this.dataService.storeToLocalStorage();

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
          legs: data.legs,
          talismans: this.viewModel.SelectedTalismans(this.armorData.talismans)
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
          legs: data.legs,
          talismans: this.viewModel.SelectedTalismans(this.armorData.talismans)
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
          legs: data.legs,
          talismans: this.viewModel.SelectedTalismans(this.armorData.talismans)
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
          legs: temporary,
          talismans: this.viewModel.SelectedTalismans(this.armorData.talismans)
        }));
      }

    }

    //console.log(chunks);
    //debugger;

    if (chunks.length < this.viewModel.numberOfThreads) { //there are more threads than there is data chunks.
      while (chunks.length < this.viewModel.numberOfThreads) {
        chunks.push(new ArmorDataDto({ //dummy chunks
          head: [],
          chest: [],
          gauntlets: [],
          legs: []
        }))
      }
    }

    if (chunks.length != this.viewModel.numberOfThreads || this.workers.length != this.viewModel.numberOfThreads) {
      this.dialog.open(ErrorDialogComponent, { data: new ErrorDialogData({ errorText: "My math messed up." }) });
      return;
    }

    this.viewModel.calcTotal(this.armorData);

    for (let i = 0; i < this.viewModel.numberOfThreads; i++) {
      this.workers[i].postMessage(new OptimizerWorkerRQ(chunks[i], this.viewModel, i));
    }
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

      //console.log(currentProgress);
      this.progressValue = Math.floor(currentProgress);

    } else if (rs.type == OptimizerWorkerRSEnum.Finished) {

      this.progressDictionary[rs.workerIndex].status = OptimizerWorkerRSEnum.Finished;

      let responseResults = rs.results.map(x =>
        new ArmorCombo(
          this.armorData.head.find(p => p.armorPieceId == x.headPieceId)!,
          this.armorData.chest.find(p => p.armorPieceId == x.chestPieceId)!,
          this.armorData.gauntlets.find(p => p.armorPieceId == x.gauntletsPieceId)!,
          this.armorData.legs.find(p => p.armorPieceId == x.legsPieceId)!,
          this.viewModel, this.viewModel.SelectedTalismans(this.armorData.talismans))
      );

      //console.log(this.results);
      //console.log(responseResults);

      //same code as optimizer.worker.ts
      for (let i = 0; i < responseResults.length; i++) {

        let combo: ArmorCombo = responseResults[i];

        if (this.results.length == 0) {
          this.results.push(combo);
        } else if (this.results.length == this.viewModel.numberOfResults && combo.score <= this.results[this.results.length - 1].score) {
          continue;
        } else {

          let inserted = false;
          for (let m = 0; m < this.results.length; m++) {
            let item: ArmorCombo = this.results[m];
            if (item.score < combo.score) {
              //Push all items down, insert at m.
              this.results.splice(m, 0, combo);
              inserted = true;

              //Remove last item if necessary
              if (this.results.length > this.viewModel.numberOfResults) this.results.splice(this.results.length - 1, 1);

              break;
            }
          }

          if (this.results.length < this.viewModel.numberOfResults && !inserted) {
            this.results.push(combo);
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

        //if (this.results.length > 0) {
        //  console.log(this.results[0].head);
        //  console.log(this.results[0].chest);
        //  console.log(this.results[0].gauntlets);
        //  console.log(this.results[0].legs);
        //}

        this.isLoading = false;
      }
    }
  }

  //#endregion

  //#region Helpers

  filterArmorData(): ArmorDataDto {

    let headFilterOverride: IArmorPieceDto | null | undefined = null;
    if (this.viewModel.filterOverrideHeadName) {
      headFilterOverride = this.armorData.head.find(x => x.name == this.viewModel.filterOverrideHeadName);
      if (headFilterOverride == null) this.viewModel.filterOverrideHeadName = null; //garbage
    }

    let chestFilterOverride: IArmorPieceDto | null | undefined = null;
    if (this.viewModel.filterOverrideChestName) {
      chestFilterOverride = this.armorData.chest.find(x => x.name == this.viewModel.filterOverrideChestName);
      if (chestFilterOverride == null) this.viewModel.filterOverrideChestName = null; //garbage
    }

    let gauntletsFilterOverride: IArmorPieceDto | null | undefined = null;
    if (this.viewModel.filterOverrideGauntletsName) {
      gauntletsFilterOverride = this.armorData.gauntlets.find(x => x.name == this.viewModel.filterOverrideGauntletsName);
      if (gauntletsFilterOverride == null) this.viewModel.filterOverrideGauntletsName = null; //garbage
    }

    let legsFilterOverride: IArmorPieceDto | null | undefined = null;
    if (this.viewModel.filterOverrideLegsName) {
      legsFilterOverride = this.armorData.legs.find(x => x.name == this.viewModel.filterOverrideLegsName);
      if (legsFilterOverride == null) this.viewModel.filterOverrideLegsName = null; //garbage
    }

    return new ArmorDataDto({
      head: headFilterOverride != null ? [headFilterOverride] : this.armorData.head.filter(x => x.isEnabled),
      chest: chestFilterOverride != null ? [chestFilterOverride] : this.armorData.chest.filter(x => x.isEnabled),
      gauntlets: gauntletsFilterOverride != null ? [gauntletsFilterOverride] : this.armorData.gauntlets.filter(x => x.isEnabled),
      legs: legsFilterOverride != null ? [legsFilterOverride] : this.armorData.legs.filter(x => x.isEnabled)
    });
  }

  getURL(piece: IArmorPieceDto): string | null {
    return UtilityHelper.getURL(piece);
  }

  getResultsCancelButtonAria(piece: IArmorPieceDto): string {
    let isArmorFilterOverride: boolean = false;

    switch (piece.type) {
      case ArmorPieceTypeEnum.Head: {
        isArmorFilterOverride = this.viewModel.filterOverrideHeadName == piece.name;
        break;
      }
      case ArmorPieceTypeEnum.Chest: {
        isArmorFilterOverride = this.viewModel.filterOverrideChestName == piece.name;
        break;
      }
      case ArmorPieceTypeEnum.Gauntlets: {
        isArmorFilterOverride = this.viewModel.filterOverrideGauntletsName == piece.name;
        break;
      }
      case ArmorPieceTypeEnum.Legs: {
        isArmorFilterOverride = this.viewModel.filterOverrideLegsName == piece.name;
        break;
      }
    }

    if (isArmorFilterOverride) {
      return `Remove ${ArmorPieceTypeEnum[piece.type]} armor filter override and recalculate`;
    } else {
      return `Disable ${piece.name} and recalculate`;
    }
  }

  setNumberOfDisabledPieces() {
    let count = 0;
    this.armorData.head.forEach(x => { if (!x.isEnabled) count++; });
    this.armorData.chest.forEach(x => { if (!x.isEnabled) count++; });
    this.armorData.gauntlets.forEach(x => { if (!x.isEnabled) count++; });
    this.armorData.legs.forEach(x => { if (!x.isEnabled) count++; });

    this.numberOfDisabledPieces = count;
  }

  filterTalismans(name: string): ITalismanDto[] {
    if (name) {
      const filterValue = name.toLowerCase();
      return this.armorData.talismans.filter(t => t.name.toLowerCase().includes(filterValue));
    } else {
      return this.armorData.talismans.slice();
    }
  }

  displayTalisman(t: ITalismanDto): string {
    return t && t.name ? t.name : "";
  }

  saveTalismanIds() {

    if (typeof this.txtTalisman1.value == "string") this.viewModel.talisman1Id = null;
    else this.viewModel.talisman1Id = (this.txtTalisman1.value as ITalismanDto).talismanId;

    if (typeof this.txtTalisman2.value == "string") this.viewModel.talisman2Id = null;
    else this.viewModel.talisman2Id = (this.txtTalisman2.value as ITalismanDto).talismanId;

    if (typeof this.txtTalisman3.value == "string") this.viewModel.talisman3Id = null;
    else this.viewModel.talisman3Id = (this.txtTalisman3.value as ITalismanDto).talismanId;

    if (typeof this.txtTalisman4.value == "string") this.viewModel.talisman4Id = null;
    else this.viewModel.talisman4Id = (this.txtTalisman4.value as ITalismanDto).talismanId;

  }

  bindTalismanAutocompletes() {
    if (this.viewModel.talisman1Id) this.txtTalisman1.setValue(this.armorData.talismans.find(x => x.talismanId == this.viewModel.talisman1Id));
    else this.txtTalisman1.setValue("");

    if (this.viewModel.talisman2Id) this.txtTalisman2.setValue(this.armorData.talismans.find(x => x.talismanId == this.viewModel.talisman2Id));
    else this.txtTalisman2.setValue("");

    if (this.viewModel.talisman3Id) this.txtTalisman3.setValue(this.armorData.talismans.find(x => x.talismanId == this.viewModel.talisman3Id));
    else this.txtTalisman3.setValue("");

    if (this.viewModel.talisman4Id) this.txtTalisman4.setValue(this.armorData.talismans.find(x => x.talismanId == this.viewModel.talisman4Id));
    else this.txtTalisman4.setValue("");
  }  

  armorOverridesList(): string {
    let strings = [];
    if (this.viewModel.filterOverrideHeadName) strings.push("Head");
    if (this.viewModel.filterOverrideChestName) strings.push("Chest");
    if (this.viewModel.filterOverrideGauntletsName) strings.push("Gauntlets");
    if (this.viewModel.filterOverrideLegsName) strings.push("Legs");

    if (strings.length == 0) {
      return "No";
    } else {
      return strings.join(", ");
    }
  }

  //#endregion
}
