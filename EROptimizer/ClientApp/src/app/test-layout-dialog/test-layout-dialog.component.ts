import { Component, OnInit } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-test-layout-dialog',
  templateUrl: './test-layout-dialog.component.html',
  styleUrls: ['./test-layout-dialog.component.css']
})
export class TestLayoutDialogComponent implements OnInit {

  array: TestObject[] = [];

  constructor(private dialogRef: MatDialogRef<TestLayoutDialogComponent>) {
    dialogRef.disableClose = true;

    for (let i = 1; i <= 33; i++) {
      this.array.push(new TestObject({ value: i, name: "Name " + i }));
    }
  }

  ngOnInit(): void {
  }

}

export class TestObject {
  value!: number;
  name!: string;

  public constructor(init?: Partial<TestObject>) {
    Object.assign(this, init);
  }
}
