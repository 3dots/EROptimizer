import { IArmorDataDto } from "../../../service/dto/IArmorDataDto";
import { OptimizerConfigDto } from "./OptimizerConfigDto";

export class OptimizerWorkerRQ implements IOptimizerWorkerRQ {
  public constructor(
    public data: IArmorDataDto,
    public config: OptimizerConfigDto,
    public workerIndex: number) { }
}

export interface IOptimizerWorkerRQ {
  config: OptimizerConfigDto;
  data: IArmorDataDto;
  workerIndex: number;
}
