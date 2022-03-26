import { ERBuild } from "../../app/best-class/model/ERBuild";
import { OptimizerConfigDto } from "../../app/optimizer/model/OptimizerConfigDto";


export class LocalStorageModel {

  config!: OptimizerConfigDto;
  build: ERBuild | null = null;

}
