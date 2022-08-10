
export class UtilityHelper {
  static getResponsiveTableStyle(tableClass: string, breakpoint: string, mobileWidth: string = "600px"): string {
    let styleString = `@media screen and (max-width: ${breakpoint}) {
      table.${tableClass} {
        width: ${mobileWidth};
      }

      table.${tableClass} > thead > tr,
      table.${tableClass} > thead > tr > th,
      table.${tableClass} > tbody > tr,
      table.${tableClass} > tbody > tr > td {
        display: block;
      }

      table.${tableClass} > thead > tr > th {
        padding: 0;
        border: 0;
      }

      table.${tableClass} > thead > tr > th > span {
        border: none;
        clip: rect(0 0 0 0);
        height: 1px;
        margin: -1px;
        overflow: hidden;
        padding: 0;
        position: absolute;
        width: 1px;
      }

      table.${tableClass} > tbody > tr > td {
        position: relative;
        padding-left: calc(50% + 0.5rem);
      }

      table.${tableClass} > tbody > tr > td:first-child {
        border-top: 2px solid currentColor;
      }

      table.${tableClass} > tbody > tr > td:last-child {
        border-bottom: none;
      }

      table.${tableClass} > tbody > tr:last-child > td:last-child {
        border-bottom: 2px solid currentColor;
      }

      table.${tableClass} > tbody > tr > td:before {
        content: attr(data-label);
        position: absolute;
        top: 50%;
        left: 0.5rem;
        transform: translate(0, -50%);
        width: calc(50% - 0.5rem);
        font-weight: bold;
      }
    }`;

    return styleString;
  }
}
