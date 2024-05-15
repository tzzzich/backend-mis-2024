using Microsoft.EntityFrameworkCore;

namespace Backend_aspnet_lab.Utils.Service
{
    public class InspectionService
    {
        public async Task<Guid?> GetBaseInspectionId(Guid currentInspectionId, ApplicationDbContext _context)
        {
            Guid baseInspectionId = currentInspectionId;

            while (true)
            {
                var currentInspection = await _context.Inspections
                    .FirstOrDefaultAsync(i => i.Id == baseInspectionId);

                if (currentInspection == null)
                {
                    return null;
                }

                if (currentInspection.PreviousInspectionId == null)
                {
                    return baseInspectionId;
                }

                baseInspectionId = currentInspection.PreviousInspectionId.Value;
            }
        }
    }
}
