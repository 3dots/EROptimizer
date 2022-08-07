import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { OptimizerConfigDto } from '../optimizer/model/OptimizerConfigDto';
import { IArmorSetDto } from '../../service/dto/IArmorSetDto';

export class PrioritizationHelpData {
  public constructor(
    public viewModel: OptimizerConfigDto,
    public setA: IArmorSetDto,
    public setB: IArmorSetDto) {
  }
}

@Component({
  selector: 'app-prioritization-help',
  templateUrl: './prioritization-help.component.html',
  styleUrls: ['./prioritization-help.component.css']
})
export class PrioritizationHelpComponent {

  constructor(@Inject(MAT_DIALOG_DATA) public data: PrioritizationHelpData, private dialogRef: MatDialogRef<PrioritizationHelpComponent>) {    

  }

  comparisonCharacter(a: number, b: number): string {
    if (a > b)
      return ">";
    else if (a == b)
      return "=";
    else
      return "<";
  }
}
