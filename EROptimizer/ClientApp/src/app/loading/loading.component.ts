import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-loading',
  templateUrl: './loading.component.html',
  styleUrls: ['./loading.component.css'],
})
export class LoadingComponent {
  @Input() hasProgressBar: boolean = false;
  @Input() progressValue: number = 0;
  @Input() progressLabel: string = "Loading";
}
