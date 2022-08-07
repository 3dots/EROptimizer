import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatAutocompleteModule } from '@angular/material/autocomplete';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { LoadingComponent } from './loading/loading.component';
import { OptimizerComponent } from './optimizer/optimizer.component';
import { AdminComponent } from './admin/admin.component';
import { ErrorDialogComponent } from './error-dialog/error-dialog.component';
import { ArmorSelectionsComponent } from './armor-selections/armor-selections.component';
import { ArmorPiecesComponent } from './armor-pieces/armor-pieces.component';
import { BestClassComponent } from './best-class/best-class.component';
import { PrioritizationHelpComponent } from './prioritization-help/prioritization-help.component';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    OptimizerComponent,
    AdminComponent,
    LoadingComponent,
    ErrorDialogComponent,
    ArmorSelectionsComponent,
    ArmorPiecesComponent,
    BestClassComponent,
    PrioritizationHelpComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    BrowserAnimationsModule,
    FormsModule,
    AppRoutingModule,
    ReactiveFormsModule,

    MatProgressBarModule,
    MatInputModule,
    MatFormFieldModule,
    MatDialogModule,
    MatButtonModule,
    MatAutocompleteModule,
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
