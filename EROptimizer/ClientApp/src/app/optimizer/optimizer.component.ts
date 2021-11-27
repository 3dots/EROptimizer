import { Component, OnInit } from '@angular/core';

import { DataService } from '../../model/data.service'
import { ArmorDataDto } from '../../model/dto/ArmorDataDto';

@Component({
  selector: 'app-optimizer',
  templateUrl: './optimizer.component.html',
  styleUrls: ['./optimizer.component.css'],
})
export class OptimizerComponent implements OnInit {

  isLoading: boolean = true;

  armorData!: ArmorDataDto;

  strength: number = 10;
  weightFractionGoal: number = 0.5;

  public get maxWeight() {
    return this.strength * 2; //todo get correct formula
  }

  public get totalAvailableWeight() {
    return this.maxWeight * this.weightFractionGoal;
  }

  constructor(private dataService: DataService) {

  }

  ngOnInit(): void {

    this.dataService.getArmorData().subscribe((data: ArmorDataDto) => {
      this.armorData = data;
      this.isLoading = false;
    });

  }

}
