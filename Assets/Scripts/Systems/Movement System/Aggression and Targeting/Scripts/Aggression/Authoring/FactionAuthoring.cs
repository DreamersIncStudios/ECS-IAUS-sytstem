using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


namespace CharacterAlignmentSystem
{
    [RequireComponent(typeof(ConvertToEntity))]
    public class FactionAuthoring : MonoBehaviour,IConvertGameObjectToEntity
    {
      
        public Alignment Faction;
        public FactionAggression AggressionMod;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            FactionAggression aggressionValue = new FactionAggression();
            switch (Faction) {
                case Alignment.Angel:
                    aggressionValue.Angels = AggressionMod.Angels -10;
                    aggressionValue.Daemons = AggressionMod.Daemons +10 ;
                    aggressionValue.Humans = AggressionMod.Humans -5;
                    aggressionValue.Mechas= AggressionMod.Mechas +0;

                    var angel = new Angel() { Corrupted =false,Aggression =aggressionValue};
                    dstManager.AddComponentData(entity, angel);
                    break;
                case Alignment.Daemon:
                    aggressionValue.Angels = AggressionMod.Angels + 10;
                    aggressionValue.Daemons = AggressionMod.Daemons - 10;
                    aggressionValue.Humans = AggressionMod.Humans + 5;
                    aggressionValue.Mechas = AggressionMod.Mechas + 0;

                    var daemon = new Daemon() { Aggression = aggressionValue };
                    dstManager.AddComponentData(entity, daemon);
                    break;
                case Alignment.Human:
                    aggressionValue.Angels = AggressionMod.Angels - 10;
                    aggressionValue.Daemons = AggressionMod.Daemons + 10;
                    aggressionValue.Humans = AggressionMod.Humans + 5;
                    aggressionValue.Mechas = AggressionMod.Mechas + 60;

                    var human = new Human() { Corrupted = false, Aggression = aggressionValue };
                    dstManager.AddComponentData(entity, human);
                    break;
                case Alignment.Mecha:
                    aggressionValue.Angels = AggressionMod.Angels +0;
                    aggressionValue.Daemons = AggressionMod.Daemons + 7;
                    aggressionValue.Humans = AggressionMod.Humans + 0;
                    aggressionValue.Mechas = AggressionMod.Mechas + 0;

                    var mecha= new Mecha() { Corrupted = false, Aggression = aggressionValue };
                    dstManager.AddComponentData(entity, mecha);
                    break;          
            }
        }
    }
}