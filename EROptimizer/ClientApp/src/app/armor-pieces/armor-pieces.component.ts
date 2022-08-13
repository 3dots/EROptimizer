import { Component, OnInit } from '@angular/core';
import { DataService } from '../../service/data.service';
import { IArmorDataDto } from '../../service/dto/IArmorDataDto';
import { ArmorPieceTypeEnum, IArmorPieceDto } from '../../service/dto/IArmorPieceDto';
import { ErrorDialogComponent, ErrorDialogData } from '../error-dialog/error-dialog.component';
import { DialogHelper } from '../utility/dialog-helper';

@Component({
  selector: 'app-armor-pieces',
  templateUrl: './armor-pieces.component.html',
  styleUrls: ['./armor-pieces.component.scss']
})
export class ArmorPiecesComponent implements OnInit {

  isLoading: boolean = true;

  armorData!: IArmorDataDto;

  ArmorPieceTypeEnum = ArmorPieceTypeEnum;
  armorType: ArmorPieceTypeEnum = ArmorPieceTypeEnum.Head;
  armorPieces: IArmorPieceDto[] = [];

  constructor(private dataService: DataService, private dialog: DialogHelper) {
  }

  ngOnInit(): void {
    this.dataService.armorData.subscribe((data: IArmorDataDto) => {
      this.armorData = data;
      this.switchToType(this.armorType);
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

  switchToType(type: ArmorPieceTypeEnum) {
    this.armorType = type;

    switch (type) {
      case ArmorPieceTypeEnum.Head: {
        this.armorPieces = this.armorData.head;
        break;
      }
      case ArmorPieceTypeEnum.Chest: {
        this.armorPieces = this.armorData.chest;
        break;
      }
      case ArmorPieceTypeEnum.Gauntlets: {
        this.armorPieces = this.armorData.gauntlets;
        break;
      }
      case ArmorPieceTypeEnum.Legs: {
        this.armorPieces = this.armorData.legs;
        break;
      }
    }
  }

  enableArmorPiece(piece: IArmorPieceDto, enable: boolean) {
    piece.isEnabled = enable;
  }

  getURL(piece: IArmorPieceDto): string | null {
    if (piece.resourceName) return "https://eldenring.wiki.fextralife.com" + piece.resourceName;
    else return null;
  }
}
