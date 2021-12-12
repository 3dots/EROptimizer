import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';

import { DataService } from '../../service/data.service'
import { IArmorDataDto } from '../../service/dto/IArmorDataDto';
import { ErrorDialogComponent } from '../error-dialog/error-dialog.component';
import { IOptimizerWorkerRS } from './model/OptimizerWorkerRS';

import { OptimizerConfigDto } from './model/OptimizerConfigDto';
import { OptimizerWorkerRQ } from './model/OptimizerWorkerRQ';

@Component({
  selector: 'app-optimizer',
  templateUrl: './optimizer.component.html',
  styleUrls: ['./optimizer.component.css'],
})
export class OptimizerComponent implements OnInit {

  isLoading: boolean = true;

  armorData!: IArmorDataDto;

  viewModel: OptimizerConfigDto = new OptimizerConfigDto();

  worker!: Worker;

  constructor(private dataService: DataService, private dialog: MatDialog) {

  }

  ngOnInit(): void {

    this.dataService.armorData.subscribe((data: IArmorDataDto) => {
      this.armorData = data;
      this.isLoading = false;

      if (typeof Worker === 'undefined') {
        this.dialog.open(ErrorDialogComponent, {
          data: {
            errorText: "Web Worker API is not supported on this browser. Please use a more modern browser",
          }
        });
      }      

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

  runOptimization(): void {
    
    this.worker = new Worker(new URL('./optimizer.worker', import.meta.url));
    this.worker.onmessage = (e: MessageEvent<IOptimizerWorkerRS>) => {
      console.log("View recieved worker message");
      console.log(e.data);
    };

    this.worker.postMessage(new OptimizerWorkerRQ(this.armorData, this.viewModel));
  }

}
