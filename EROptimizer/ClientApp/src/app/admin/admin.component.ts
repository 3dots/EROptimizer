import { Component, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import * as signalR from "@microsoft/signalr";

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  /*styleUrls: ['./admin.component.css'],*/
})
export class AdminComponent implements AfterViewChecked {
  

  @ViewChild('scrapeConsole') scrapeConsole!: ElementRef;

  isScrapeButtonDisabled: boolean = false;
  scrapeConsoleText: string = "";
  isScrapeScrollUpdate: boolean = false;

  socket!: signalR.HubConnection;

  ngAfterViewChecked(): void {
    if (this.isScrapeScrollUpdate) {
      this.isScrapeScrollUpdate = false;
      this.scrapeConsole.nativeElement.scrollTop = this.scrapeConsole.nativeElement.scrollHeight;
    }
  }

  scrape() {

    this.isScrapeButtonDisabled = true;

    this.socket = new signalR.HubConnectionBuilder().withUrl("/scrapeWikiHub").build();
    this.socket.on("WriteLine", this.onRecieveMessage.bind(this));
    this.socket.on("ScrapeEnd", this.onScrapeEnd.bind(this));

    this.socket.start().then(() => {
      this.socket.invoke("StartScrape").catch(this.onSocketError);
    }).catch(this.onSocketError);
  }

  onRecieveMessage(message: string) {
    this.scrapeConsoleText += "\n" + message;
    this.isScrapeScrollUpdate = true;
  }

  onSocketError(err: any) {
    this.scrapeConsoleText = err.toString();
    this.isScrapeButtonDisabled = false;
  }

  onScrapeEnd() {
    this.socket.stop().then(() => {
      this.isScrapeButtonDisabled = false;
    });    
  }
}
