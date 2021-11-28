import { Component, OnInit } from '@angular/core';

import { DataService } from '../../service/data.service'
import { IArmorDataDto } from '../../service/dto/IArmorDataDto';

import { OptimizerConfigDto } from './model/OptimizerConfigDto';

@Component({
  selector: 'app-optimizer',
  templateUrl: './optimizer.component.html',
  styleUrls: ['./optimizer.component.css'],
})
export class OptimizerComponent implements OnInit {

  isLoading: boolean = true;

  armorData!: IArmorDataDto;

  viewModel: OptimizerConfigDto = new OptimizerConfigDto();

  constructor(private dataService: DataService) {

  }

  ngOnInit(): void {

    this.dataService.getArmorData().subscribe((data: IArmorDataDto) => {
      this.armorData = data;
      this.isLoading = false;
    });

  }

}
