import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';

import { DataService } from '../../service/data.service'
import { IArmorDataDto } from '../../service/dto/IArmorDataDto';
import { ErrorDialogComponent } from '../error-dialog/error-dialog.component';

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

  worker!: Worker;

  constructor(private dataService: DataService, public dialog: MatDialog) {

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
        return;
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
    this.worker.onmessage = ({ data }) => {
      console.log(`page got message: ${data}`);
    };


    this.worker.postMessage({ key: "hello" });
  }

}
