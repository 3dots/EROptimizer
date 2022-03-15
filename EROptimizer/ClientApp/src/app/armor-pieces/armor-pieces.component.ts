import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { DataService } from '../../service/data.service';
import { IArmorDataDto } from '../../service/dto/IArmorDataDto';
import { ArmorPieceTypeEnum } from '../../service/dto/IArmorPieceDto';
import { ErrorDialogComponent } from '../error-dialog/error-dialog.component';

@Component({
  selector: 'app-armor-pieces',
  templateUrl: './armor-pieces.component.html',
  styleUrls: ['./armor-pieces.component.css']
})
export class ArmorPiecesComponent implements OnInit {

  isLoading: boolean = true;

  armorData!: IArmorDataDto;

  ArmorPieceTypeEnum = ArmorPieceTypeEnum;
  armorType: ArmorPieceTypeEnum = ArmorPieceTypeEnum.Head;

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

  switchToType(type: ArmorPieceTypeEnum) {
    this.armorType = type;
  }

}
