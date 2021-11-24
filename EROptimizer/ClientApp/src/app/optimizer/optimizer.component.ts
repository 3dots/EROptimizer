import { Component, OnInit } from '@angular/core';

import { DataService } from '../../model/data.service'

@Component({
  selector: 'app-optimizer',
  templateUrl: './optimizer.component.html',
  styleUrls: ['./optimizer.component.css'],
})
export class OptimizerComponent implements OnInit {

  isLoading: boolean = true;

  ngOnInit(): void {

    setTimeout(() => {
      //this.isLoading = false;
    }, 3000);

  }

  constructor(dataService: DataService) {

  }
}
