import { IArmorPieceDto } from "../../service/dto/IArmorPieceDto";

export class UtilityHelper {
  static getURL(piece: IArmorPieceDto): string | null {
    if (piece.resourceName) return "https://eldenring.wiki.fextralife.com" + piece.resourceName;
    else return null;
  }
}
