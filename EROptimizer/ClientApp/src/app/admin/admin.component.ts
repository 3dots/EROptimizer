import { Component, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import * as signalR from "@microsoft/signalr";
import { HubConnectionState } from '@microsoft/signalr';
import { TestLayoutDialogComponent } from '../test-layout-dialog/test-layout-dialog.component';
import { IArmorDataChangesDto } from './dto/IArmorDataChangesDto';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css'],
})
export class AdminComponent implements AfterViewChecked {

  socket!: signalR.HubConnection;

  isScrapeButtonDisabled: boolean = false;
  scrapeConsoleText: string = "";

  @ViewChild('scrapeConsole') scrapeConsole!: ElementRef;
  isScrapeScrollUpdate: boolean = false;

  isScrapeDisplayChanges: boolean = false;
  changes!: IArmorDataChangesDto | null;
  isSaveButtonDisabled: boolean = true;

  password: string = "";

  constructor(private dialog: MatDialog) {
    
  }

  ngAfterViewChecked(): void {
    if (this.isScrapeScrollUpdate) {
      this.isScrapeScrollUpdate = false;
      this.scrapeConsole.nativeElement.scrollTop = this.scrapeConsole.nativeElement.scrollHeight;
    }
  }

  scrape() {
    this.isScrapeButtonDisabled = true;
    this.scrapeConsoleText = "";
    this.isScrapeDisplayChanges = false;
    this.changes = null;
    this.isSaveButtonDisabled = false;

    this.socket = new signalR.HubConnectionBuilder().withUrl("/scrapeWikiHub").build();

    this.socket.onclose(() => { });

    this.socket.on("WriteLine", this.onRecieveMessage.bind(this));
    this.socket.on("DataRetrieved", this.onDataRetrieved.bind(this));
    this.socket.on("ScrapeEnd", this.onScrapeEnd.bind(this));
    this.socket.on("Denied", this.onDenied.bind(this));

    this.socket.start().then(() => {
      this.socket.invoke("StartScrape", this.password);
    });
  }

  onRecieveMessage(message: string) {
    this.scrapeConsoleText += "\n" + message;
    this.isScrapeScrollUpdate = true;
  }

  onSocketError(err: any) {

    //console.log(err);
    //console.log(this.socket.state);

    //this.onRecieveMessage(err.toString());
    //this.isScrapeButtonDisabled = false;
  }

  onScrapeEnd() {
    this.socket.stop().then(() => {
      this.isScrapeButtonDisabled = false;
      this.isSaveButtonDisabled = true;
    });    
  }

  onDataRetrieved(diff: IArmorDataChangesDto) {
    this.changes = diff;
    this.isScrapeDisplayChanges = true;
  }

  onDenied() {
    this.socket.stop();
  }

  save() {

    if (!(this.socket.state == HubConnectionState.Connected)) {
      this.socket.start().then(() => {
        this.socket.invoke("DataSave");
      });
    } else {
      this.socket.invoke("DataSave");
    }    
  }

  cancel() {
    this.onScrapeEnd();
  }

  testDialog() {
    this.dialog.open(TestLayoutDialogComponent, {
      data: {
        errorText: "test",
      },
      /*width: "100%"*/
    });
  }
}
