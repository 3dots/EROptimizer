import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { map, Observable, startWith } from 'rxjs';
import { DataService } from '../../service/data.service';
import { IArmorDataDto } from '../../service/dto/IArmorDataDto';
import { ArmorPieceTypeEnum, IArmorPieceDto } from '../../service/dto/IArmorPieceDto';
import { IArmorSetDto } from '../../service/dto/IArmorSetDto';
import { ErrorDialogComponent, ErrorDialogData } from '../error-dialog/error-dialog.component';
import { OptimizerConfigDto } from '../optimizer/model/OptimizerConfigDto';
import { DialogHelper } from '../utility/dialog-helper';
import { UtilityHelper } from '../utility/utility-helper';

@Component({
  selector: 'app-armor-filters',
  templateUrl: './armor-filters.component.html',
  styleUrls: ['./armor-filters.component.scss']
})
export class ArmorFiltersComponent implements OnInit {

  //#region Fields and Properties

  isLoading: boolean = true;

  armorData!: IArmorDataDto;
  viewModel: OptimizerConfigDto;

  @ViewChild('filterOverridesHelpDialogTemplate') filterOverridesHelpDialogTemplate!: TemplateRef<any>;

  txtOverrideHead: FormControl = new FormControl();
  filteredHead!: Observable<IArmorPieceDto[]>;
  txtOverrideChest: FormControl = new FormControl();
  filteredChest!: Observable<IArmorPieceDto[]>;
  txtOverrideGauntlets: FormControl = new FormControl();
  filteredGauntlets!: Observable<IArmorPieceDto[]>;
  txtOverrideLegs: FormControl = new FormControl();
  filteredLegs!: Observable<IArmorPieceDto[]>;

  constructor(private dataService: DataService, private dialog: DialogHelper) {
    this.viewModel = dataService.model.config;
  }

  //#endregion

  //#region Events

  ngOnInit(): void {
    this.dataService.armorData.subscribe((data: IArmorDataDto) => {
      this.armorData = data;

      this.filteredHead = this.txtOverrideHead.valueChanges.pipe(
        //tap(x => console.log(x)),
        startWith(""),
        map<string | IArmorPieceDto, string>(value => (typeof value === 'string' ? value : value.name)),
        map<string, IArmorPieceDto[]>(name => this.filterArmorPieces(name, ArmorPieceTypeEnum.Head))
      );
      this.filteredChest = this.txtOverrideChest.valueChanges.pipe(
        startWith(""),
        map<string | IArmorPieceDto, string>(value => (typeof value === 'string' ? value : value.name)),
        map<string, IArmorPieceDto[]>(name => this.filterArmorPieces(name, ArmorPieceTypeEnum.Chest))
      );
      this.filteredGauntlets = this.txtOverrideGauntlets.valueChanges.pipe(
        startWith(""),
        map<string | IArmorPieceDto, string>(value => (typeof value === 'string' ? value : value.name)),
        map<string, IArmorPieceDto[]>(name => this.filterArmorPieces(name, ArmorPieceTypeEnum.Gauntlets))
      );
      this.filteredLegs = this.txtOverrideLegs.valueChanges.pipe(
        startWith(""),
        map<string | IArmorPieceDto, string>(value => (typeof value === 'string' ? value : value.name)),
        map<string, IArmorPieceDto[]>(name => this.filterArmorPieces(name, ArmorPieceTypeEnum.Legs))
      );

      this.bindFilterOverrides();

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

  enableArmorSet(set: IArmorSetDto, enable: boolean) {
    if (set.armorSetId == 0) {
      set.combo.head.isEnabled = enable;
      set.combo.chest.isEnabled = enable;
      set.combo.gauntlets.isEnabled = enable;
      set.combo.legs.isEnabled = enable;
    } else {
      if (!set.combo.head.armorSetIds.includes(0)) set.combo.head.isEnabled = enable;
      if (!set.combo.chest.armorSetIds.includes(0)) set.combo.chest.isEnabled = enable;
      if (!set.combo.gauntlets.armorSetIds.includes(0)) set.combo.gauntlets.isEnabled = enable;
      if (!set.combo.legs.armorSetIds.includes(0)) set.combo.legs.isEnabled = enable;
    }
  }

  enableArmorPiece(piece: IArmorPieceDto, enable: boolean) {
    piece.isEnabled = enable;
  }

  enableAllHead(enable: boolean) {
    this.armorData.head.forEach((x) => {
      if (!x.armorSetIds.includes(0)) x.isEnabled = enable;
    });
  }

  enableAllChest(enable: boolean) {
    this.armorData.chest.forEach((x) => {
      if (!x.armorSetIds.includes(0)) x.isEnabled = enable;
    });
  }

  enableAllGauntlets(enable: boolean) {
    this.armorData.gauntlets.forEach((x) => {
      if (!x.armorSetIds.includes(0)) x.isEnabled = enable;
    });
  }

  enableAllLegs(enable: boolean) {
    this.armorData.legs.forEach((x) => {
      if (!x.armorSetIds.includes(0)) x.isEnabled = enable;
    });
  }

  enableAll(enable: boolean) {
    this.armorData.head.forEach(x => {
      if (enable) x.isEnabled = true;
      else if (!x.armorSetIds.includes(0)) x.isEnabled = false;
    });
    this.armorData.chest.forEach(x => {
      if (enable) x.isEnabled = true;
      else if (!x.armorSetIds.includes(0)) x.isEnabled = false;
    });
    this.armorData.gauntlets.forEach(x => {
      if (enable) x.isEnabled = true;
      else if (!x.armorSetIds.includes(0)) x.isEnabled = false;
    });
    this.armorData.legs.forEach(x => {
      if (enable) x.isEnabled = true;
      else if (!x.armorSetIds.includes(0)) x.isEnabled = false;
    });
  }

  filterOverridesHelpDialog() {
    this.dialog.open(this.filterOverridesHelpDialogTemplate, {
    });
  }

  onOverrideFilterChanged(ev: MatAutocompleteSelectedEvent) {
    this.saveFilterOverrides();
  }

  clearAutocomplete(txtAutocomplete: FormControl) {
    txtAutocomplete.setValue("");
    this.saveFilterOverrides();
  }

  //#endregion

  //#region Helpers

  getURL(piece: IArmorPieceDto): string | null {
    return UtilityHelper.getURL(piece);
  }

  linkDecorationStyle(piece: IArmorPieceDto): string {
    if (piece.name == "None") return 'none';
    else if (piece.isEnabled) return 'underline';
    else return 'line-through';
  }

  displayArmorPiece(p: IArmorPieceDto): string {
    return p && p.name ? p.name : "";
  }

  filterArmorPieces(name: string, type: ArmorPieceTypeEnum): IArmorPieceDto[] {

    let pieces: IArmorPieceDto[] = [];
    switch (type) {
      case ArmorPieceTypeEnum.Head: {
        pieces = this.armorData.head;
        break;
      }
      case ArmorPieceTypeEnum.Chest: {
        pieces = this.armorData.chest;
        break;
      }
      case ArmorPieceTypeEnum.Gauntlets: {
        pieces = this.armorData.gauntlets;
        break;
      }
      case ArmorPieceTypeEnum.Legs: {
        pieces = this.armorData.legs;
        break;
      }
    }

    if (name) {
      const filterValue = name.toLowerCase();
      return pieces.filter(t => t.name.toLowerCase().includes(filterValue));
    } else {
      return pieces.slice();
    }
  }

  saveFilterOverrides() {

    if (typeof this.txtOverrideHead.value == "string") this.viewModel.filterOverrideHeadName = null;
    else this.viewModel.filterOverrideHeadName = (this.txtOverrideHead.value as IArmorPieceDto).name;

    if (typeof this.txtOverrideChest.value == "string") this.viewModel.filterOverrideChestName = null;
    else this.viewModel.filterOverrideChestName = (this.txtOverrideChest.value as IArmorPieceDto).name;

    if (typeof this.txtOverrideGauntlets.value == "string") this.viewModel.filterOverrideGauntletsName = null;
    else this.viewModel.filterOverrideGauntletsName = (this.txtOverrideGauntlets.value as IArmorPieceDto).name;

    if (typeof this.txtOverrideLegs.value == "string") this.viewModel.filterOverrideLegsName = null;
    else this.viewModel.filterOverrideLegsName = (this.txtOverrideLegs.value as IArmorPieceDto).name;

    this.dataService.storeToLocalStorage();
  }

  bindFilterOverrides() {

    if (this.viewModel.filterOverrideHeadName) this.txtOverrideHead.setValue(this.armorData.head.find(x => x.name == this.viewModel.filterOverrideHeadName));
    else this.txtOverrideHead.setValue("");

    if (this.viewModel.filterOverrideChestName) this.txtOverrideChest.setValue(this.armorData.chest.find(x => x.name == this.viewModel.filterOverrideChestName));
    else this.txtOverrideChest.setValue("");

    if (this.viewModel.filterOverrideGauntletsName) this.txtOverrideGauntlets.setValue(this.armorData.gauntlets.find(x => x.name == this.viewModel.filterOverrideGauntletsName));
    else this.txtOverrideGauntlets.setValue("");

    if (this.viewModel.filterOverrideLegsName) this.txtOverrideLegs.setValue(this.armorData.legs.find(x => x.name == this.viewModel.filterOverrideLegsName));
    else this.txtOverrideLegs.setValue("");
  }

  //#endregion
}
