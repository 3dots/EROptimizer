import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { DataService } from '../../service/data.service';
import { IArmorDataDto } from '../../service/dto/IArmorDataDto';
import { IArmorSetDto } from '../../service/dto/IArmorSetDto';
import { ErrorDialogComponent } from '../error-dialog/error-dialog.component';

@Component({
  selector: 'app-armor-selections',
  templateUrl: './armor-selections.component.html',
  styleUrls: ['./armor-selections.component.css']
})
export class ArmorSelectionsComponent implements OnInit {

  isLoading: boolean = true;

  armorData!: IArmorDataDto;

  constructor(private dataService: DataService, private dialog: MatDialog) {

  }

  ngOnInit(): void {
    this.dataService.armorData.subscribe((data: IArmorDataDto) => {
      this.armorData = data;
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

  enableArmorSet(set: IArmorSetDto, enable: boolean) {
    if (set.armorSetId == 0) {
      set.combo.head.isEnabled = enable;
      set.combo.chest.isEnabled = enable;
      set.combo.gauntlets.isEnabled = enable;
      set.combo.legs.isEnabled = enable;
    } else {
      if (set.combo.head.armorSetId > 0) set.combo.head.isEnabled = enable;
      if (set.combo.chest.armorSetId > 0) set.combo.chest.isEnabled = enable;
      if (set.combo.gauntlets.armorSetId > 0) set.combo.gauntlets.isEnabled = enable;
      if (set.combo.legs.armorSetId > 0) set.combo.legs.isEnabled = enable;
    }
    console.log(set);
  }
}
