import { HttpErrorResponse } from '@angular/common/http';
import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

interface ErrorDialogData {
  errorTitle: string;
  errorText: string;
  errorException: any;
}

@Component({
  selector: 'app-error-dialog',
  templateUrl: './error-dialog.component.html',
  styleUrls: ['./error-dialog.component.css']
})
export class ErrorDialogComponent {

  exceptionText: string | undefined;

  constructor(@Inject(MAT_DIALOG_DATA) public errorData: ErrorDialogData, private dialogRef: MatDialogRef<ErrorDialogComponent>) {
    dialogRef.disableClose = true;

    errorData.errorText = errorData.errorText ?? "Error";
    errorData.errorTitle = errorData.errorTitle ?? "Error";

    if (errorData.errorException instanceof HttpErrorResponse) {
      this.exceptionText = errorData.errorException.message;
    }
  }
}
