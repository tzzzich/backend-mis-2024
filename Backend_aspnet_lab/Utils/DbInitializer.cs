using Backend_aspnet_lab.Models;
using Backend_aspnet_lab.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Backend_aspnet_lab
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context)
        {
            context.Database.Migrate();
            await FillSpecialities(context);
            await FillIcd10(context);
        }

        private static async Task FillSpecialities(ApplicationDbContext context)
        {
            if (context.Specialities.Any())
            {
                return;
            }

            var specialities = new List<Speciality>
            {
                new Speciality { Name = "Акушер-гинеколог" , CreateTime = DateTimeOffset.UtcNow},
                new Speciality { Name = "Анестезиолог-реаниматолог", CreateTime = DateTimeOffset.UtcNow},
                new Speciality { Name = "Дерматовенеролог", CreateTime = DateTimeOffset.UtcNow },
                new Speciality { Name = "Инфекционист" , CreateTime = DateTimeOffset.UtcNow},
                new Speciality { Name = "Кардиолог" , CreateTime = DateTimeOffset.UtcNow},
                new Speciality { Name = "Невролог" , CreateTime = DateTimeOffset.UtcNow},
                new Speciality { Name = "Онколог" , CreateTime = DateTimeOffset.UtcNow},
                new Speciality { Name = "Отоларинголог" , CreateTime = DateTimeOffset.UtcNow},
                new Speciality { Name = "Офтальмолог" , CreateTime = DateTimeOffset.UtcNow},
                new Speciality { Name = "Психиатр" , CreateTime = DateTimeOffset.UtcNow},
                new Speciality { Name = "Психолог" , CreateTime = DateTimeOffset.UtcNow},
                new Speciality { Name = "Рентгенолог" , CreateTime = DateTimeOffset.UtcNow},
                new Speciality { Name = "Стоматолог" , CreateTime = DateTimeOffset.UtcNow},
                new Speciality { Name = "Терапевт" , CreateTime = DateTimeOffset.UtcNow},
                new Speciality { Name = "УЗИ-специалист" , CreateTime = DateTimeOffset.UtcNow},
                new Speciality { Name = "Уролог" , CreateTime = DateTimeOffset.UtcNow},
                new Speciality { Name = "Хирург" , CreateTime = DateTimeOffset.UtcNow},
                new Speciality { Name = "Эндокринолог" , CreateTime = DateTimeOffset.UtcNow}

            };

            context.Specialities.AddRange(specialities);
            await context.SaveChangesAsync();
        }

        public static async Task FillIcd10(ApplicationDbContext dbContext)
        {
            if (dbContext.Icd10Records.Any())
            {
                return;
            }

            string jsonString = File.ReadAllText(@"Data/Icd10.json");
            var objectsLists = JsonConvert.DeserializeObject<dynamic>(jsonString);

            Dictionary<string, string> rootKeys = new Dictionary<string, string>()
             {
                 {"I","A00-B99"},
                 {"II","C00-D48"},
                 {"III","D50-D89"},
                 {"IV","E00-E90"},
                 {"V","F00-F99"},
                 {"VI","G00-G99"},
                 {"VII","H00-H59"},
                 {"VIII","H60-H95"},
                 {"IX","I00-I99"},
                 {"X","J00-J99"},
                 {"XI","K00-K93"},
                 {"XII","L00-L99"},
                 {"XIII","M00-M99"},
                 {"XIV","N00-N99"},
                 {"XV","O00-O99"},
                 {"XVI","P00-P96"},
                 {"XVII","Q00-Q99"},
                 {"XVIII","R00-R99"},
                 {"XIX","S00-T98"},
                 {"XX","U00-U85"},
                 {"XXI","V01-Y98"},
                 {"XXII","Z00-Z99"}
             };

            var Icd10ItemsList = new List<Icd10ParsingObjectItem>();
            foreach (dynamic item in objectsLists["records"])
            {
                if(item.ACTUAL == 0)
                {
                    continue;
                }
                var newItem = new Icd10ParsingObjectItem();
                newItem.InnerId = Guid.NewGuid();
                newItem.Mkb_code = item.MKB_CODE;
                newItem.Mkb_name = item.MKB_NAME;
                newItem.Id = item.ID;
                newItem.Id_parent = item.ID_PARENT;
                newItem.RootId = null;
                if (newItem.Mkb_code != null)
                {
                    if (rootKeys.ContainsKey(newItem.Mkb_code))
                    {
                        newItem.Mkb_code = rootKeys[newItem.Mkb_code];
                        newItem.RootId = newItem.InnerId;
                    }
                }
                Icd10ItemsList.Add(newItem);
            }
            foreach (var item in Icd10ItemsList)
            {
                if (item.Id_parent != null)
                {
                    item.ParentId = Icd10ItemsList
						.Where(i => i.Id == item.Id_parent)
                        .Select(i => i.InnerId)
						.FirstOrDefault();
				}
                else
                {
                    item.ParentId = null;
                    item.RootId = item.InnerId;
                }
            }

            foreach (var item in Icd10ItemsList.Where(x => x.ParentId == null).ToList<Icd10ParsingObjectItem>())
            {
                await AddItem(item, null, Icd10ItemsList, dbContext);
            }
        }

        public static async Task AddItem(Icd10ParsingObjectItem item, Icd10ParsingObjectItem parent, 
            List<Icd10ParsingObjectItem> Icd10ItemsList, ApplicationDbContext dbContext)
        {
            var newRecord = new Icd10Record
            {
                Id = item.InnerId,
                Code = item.Mkb_code,
                Name = item.Mkb_name,
                PreviousId = item.ParentId,
                RootId = parent != null ? parent.RootId : item.InnerId,
                CreateTime = DateTimeOffset.UtcNow
            };
            item.RootId = newRecord.RootId;

            dbContext.Set<Icd10Record>().Add(newRecord);
            await dbContext.SaveChangesAsync();

            var childRecords = Icd10ItemsList.Where(i => i.ParentId == item.InnerId).ToList<Icd10ParsingObjectItem>();
            if (childRecords.Count > 0)
            {
                foreach (var childRecord in childRecords)
                {
                    await AddItem(childRecord, item, Icd10ItemsList, dbContext);
                }
            }
        }
    }
}