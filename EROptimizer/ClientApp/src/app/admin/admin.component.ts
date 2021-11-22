import { Component } from '@angular/core';
import * as signalR from "@microsoft/signalr";

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  /*styleUrls: ['./admin.component.css'],*/
})
export class AdminComponent {

  isScrapeButtonDisabled: boolean = false;
  scrapeConsoleText: string = "";

  socket: signalR.HubConnection | null = null;

  scrape() {

    this.isScrapeButtonDisabled = true;

    this.socket = new signalR.HubConnectionBuilder().withUrl("/scrapeWikiHub").build();
    this.socket.on("ReceiveMessage", this.onRecieveMessage.bind(this));

    this.socket.start().then(() => {
      this.socket?.invoke("StartScrape")?.catch(this.onSocketError);
    }).catch(this.onSocketError);
  }

  onRecieveMessage(message: string) {
    this.scrapeConsoleText += "\n" + message;
  }

  onSocketError(err: any) {
    this.scrapeConsoleText = err.toString();
    this.isScrapeButtonDisabled = false;
  }
}
