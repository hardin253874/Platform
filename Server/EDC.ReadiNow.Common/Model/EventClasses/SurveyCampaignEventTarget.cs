using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Collections.Generic;

namespace EDC.ReadiNow.Model.EventClasses
{
    /// <summary>
    /// Updates the corresponding tasks when any changes are made to the campaign.
    /// </summary>
    public class SurveyCampaignEventTarget : IEntityEventSave, IEntityEventDelete
    {
        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            foreach (var entity in entities)
            {
                var campaign = entity.As<SurveyCampaign>();

                if (campaign == null)
                    continue;

                if (!campaign.IsTemporaryId)
                    UpdateCampaign(campaign);
            }

            return false;
        }

        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
        }

        public bool OnBeforeDelete(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            foreach (var entity in entities)
            {
                var campaign = entity.As<SurveyCampaign>();

                if (campaign == null)
                    continue;

                if (!campaign.IsTemporaryId)
                    DeleteCampaign(campaign);
            }

            return false;
        }

        public void OnAfterDelete(IEnumerable<long> entities, IDictionary<string, object> state)
        {
        }

        private void UpdateCampaign(SurveyCampaign campaign)
        {
            IEntityFieldValues fields;
            IDictionary<long, IChangeTracker<IMutableIdKey>> forwardRelationships;
            IDictionary<long, IChangeTracker<IMutableIdKey>> reverseRelationships;

            campaign.GetChanges(out fields, out forwardRelationships, out reverseRelationships);

            // handle a campaign having the due date updated.
            if (fields != null)
            {
                if (fields.ContainsField(SurveyCampaign.SurveyClosesOn_Field.Id))
                {
                    var tasks = new List<IEntity>();
                    foreach (var result in campaign.SurveyResponses)
                    {
                        foreach (var task in result.UserSurveyTaskForResults)
                        {
                            if (!(task.UserSurveyTaskForReview ?? false))
                            {
                                var writableTask = task.AsWritable<UserSurveyTask>();
                                writableTask.UserTaskDueOn = campaign.SurveyClosesOn;
                                tasks.Add(writableTask);
                            }
                        }
                    }

                    if (tasks.Any())
                        Entity.Save(tasks);
                }
            }
        }

        private void DeleteCampaign(SurveyCampaign campaign)
        {
            // delete any tasks that are out there for this campaign
            var tasks = campaign.SurveyResponses.SelectMany(r => r.UserSurveyTaskForResults)
                .Where(t => !t.IsTemporaryId)
                .Select(t => new EntityRef(t)).ToList();

            if (tasks.Any())
                Entity.Delete(tasks);
        }
    }
}
