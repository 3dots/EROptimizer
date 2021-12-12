/// <reference lib="webworker" />

import { IArmorDataDto } from "../../service/dto/IArmorDataDto";
import { IArmorPieceDto } from "../../service/dto/IArmorPieceDto";
import { ArmorCombo } from "./model/ArmorCombo";
import { OptimizerConfigDto } from "./model/OptimizerConfigDto";
import { IOptimizerWorkerRQ } from "./model/OptimizerWorkerRQ";
import { OptimizerWorkerRS, OptimizerWorkerRSEnum } from "./model/OptimizerWorkerRS";

addEventListener('message', (e: MessageEvent<IOptimizerWorkerRQ>) => {

  console.log("Worker recieved message:");
  console.log(e.data);

  let data: IArmorDataDto = e.data.data;
  let config: OptimizerConfigDto = e.data.config;

  let totalAvailableWeight = config.totalAvailableWeight;

  for (let i = 0; i < data.head.length; i++) {
    if (i > 0) postMessage(new OptimizerWorkerRS({ type: OptimizerWorkerRSEnum.Progress, progress: i / data.head.length }));

    let head: IArmorPieceDto = data.head[i];

    let weight = head.weight;
    if (weight > totalAvailableWeight) continue;

    for (let j = 0; j < data.chest.length; j++) {
      let chest: IArmorPieceDto = data.chest[j];

      weight = head.weight + chest.weight;
      if (weight > totalAvailableWeight) continue;

      for (let k = 0; k < data.gauntlets.length; k++) {
        let gauntlets: IArmorPieceDto = data.gauntlets[k];

        weight = head.weight + chest.weight + gauntlets.weight;
        if (weight > totalAvailableWeight) continue;

        for (let l = 0; l < data.legs.length; l++) {
          let legs: IArmorPieceDto = data.legs[l];

          weight = head.weight + chest.weight + gauntlets.weight + legs.weight;
          if (weight > totalAvailableWeight) continue;

          let combo = new ArmorCombo(head, chest, gauntlets, legs, config);

        }
      }

    }
  }

  //postMessage(new OptimizerWorkerRS({ type: OptimizerWorkerRSEnum.Finished, finishedResponse:  }))

});
