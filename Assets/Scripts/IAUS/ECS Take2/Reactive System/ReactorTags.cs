using Unity.Entities;

namespace Utilities.ReactiveSystem
{
    public interface IComponentReactorTagsForAIStates<COMPONENT, AICOMPONENT>
    {

        void ComponentAdded(Entity entity, ref COMPONENT newComponent, ref AICOMPONENT AIStateCompoment);
        void ComponentRemoved(Entity entity, ref AICOMPONENT AIStateCompoment, in COMPONENT oldComponent);
        void ComponentValueChanged(Entity entity, ref COMPONENT newComponent, ref AICOMPONENT AIStateCompoment, in COMPONENT oldComponent);
    }

    public interface IComponentReactorTagsForAIStates<COMPONENT, ACTIVEAICOMPONENT, UPDATINGAICOMPONET>
    {

        void ComponentAdded(Entity entity, ref COMPONENT newComponent, ref ACTIVEAICOMPONENT AIStateCompoment, ref UPDATINGAICOMPONET UpdatingComponent);
        void ComponentRemoved(Entity entity, ref ACTIVEAICOMPONENT AIStateCompoment, ref UPDATINGAICOMPONET UpdatingComponent, in COMPONENT oldComponent);
        void ComponentValueChanged(Entity entity, ref COMPONENT newComponent, ref ACTIVEAICOMPONENT AIStateCompoment, ref UPDATINGAICOMPONET UpdatingComponent,  in COMPONENT oldComponent);
    }
}