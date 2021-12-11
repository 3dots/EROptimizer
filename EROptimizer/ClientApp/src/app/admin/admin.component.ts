import { Component, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import * as signalR from "@microsoft/signalr";
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
    this.socket.on("WriteLine", this.onRecieveMessage.bind(this));
    this.socket.on("DataRetrieved", this.onDataRetrieved.bind(this));
    this.socket.on("ScrapeEnd", this.onScrapeEnd.bind(this));
    this.socket.on("Denied", this.onDenied.bind(this));

    this.socket.start().then(() => {
      this.socket.invoke("StartScrape", this.password).catch(this.onSocketError.bind(this));
    }).catch(this.onSocketError.bind(this));
  }

  onRecieveMessage(message: string) {
    this.scrapeConsoleText += "\n" + message;
    this.isScrapeScrollUpdate = true;
  }

  onSocketError(err: any) {
    this.onRecieveMessage(err.toString());
    this.isScrapeButtonDisabled = false;
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
    this.socket.invoke("DataSave").catch(this.onSocketError.bind(this));
  }

  cancel() {
    this.onScrapeEnd();
  }
}
