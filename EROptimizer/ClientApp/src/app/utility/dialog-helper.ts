import { ComponentType } from "@angular/cdk/portal";
import { Injectable, TemplateRef } from "@angular/core";
import { MatDialog, MatDialogConfig, MatDialogRef } from "@angular/material/dialog";

@Injectable({
  providedIn: 'root'
})
export class DialogHelper {

  /**
     * Opens a modal dialog containing the given component.
     * @param component Type of the component to load into the dialog.
     * @param config Extra configuration options.
     * @returns Reference to the newly-opened dialog.
     */
  open<T, D = any, R = any>(component: ComponentType<T>, config?: MatDialogConfig<D>): MatDialogRef<T, R>;
  /**
   * Opens a modal dialog containing the given template.
   * @param template TemplateRef to instantiate as the dialog content.
   * @param config Extra configuration options.
   * @returns Reference to the newly-opened dialog.
   */
  open<T, D = any, R = any>(template: TemplateRef<T>, config?: MatDialogConfig<D>): MatDialogRef<T, R>;
  open<T, D = any, R = any>(template: ComponentType<T> | TemplateRef<T>, config?: MatDialogConfig<D>): MatDialogRef<T, R> {

    if (config == null) {
      config = {}
    }

    if (config.maxWidth == null) {
      config.maxWidth = "95vw";
    }

    return this.dialog.open(template, config);
  };

  constructor(private dialog: MatDialog) {
    
  }
}
