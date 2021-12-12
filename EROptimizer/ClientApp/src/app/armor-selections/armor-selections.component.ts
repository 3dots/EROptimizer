import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { DataService } from '../../service/data.service';
import { IArmorDataDto } from '../../service/dto/IArmorDataDto';
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

}
