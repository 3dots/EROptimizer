import { IArmorDataDto } from "../../../service/dto/IArmorDataDto";
import { OptimizerConfigDto } from "./OptimizerConfigDto";

export interface IOptimizerWorkerRQ {
  config: OptimizerConfigDto;
  data: IArmorDataDto;
}
