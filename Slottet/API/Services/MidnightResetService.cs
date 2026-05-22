using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    //Baggrundsservice der kører ved midnat og nulstiller alle medicins IsTaken til false, så de er klar til en ny dag og fluebenet fjernes
    //Backgroundservice er en service der kører i baggrunden af applikationen, og brugest til at udfører opgaver på bestemte tidspukter. 
    //Den åbner når API startes og lukkes når API lukkes
    public class MidnightResetService : BackgroundService
    {
        //ServiceScopeFactory skal bruges for at kunne oprette en scope for at få adgang til databasen
        private readonly IServiceScopeFactory _serviceScopeFactory;
        

        //Constructor der tager en ServiceScopeFactory som parameter, og gemmer den i en privat field
        public MidnightResetService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        //ExecuteAsync er den metode der kører når service startes, og den indeholder logikken for at nulstille medicins IsTaken til false ved midnat
        //cancellationToken bruges til at stoppe service når API lukkes
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            //While loop der kører indtil cancellationToken bliver cancelled, altså indtil API lukkes
            while (!cancellationToken.IsCancellationRequested)
            {
                //beregn hvor lang tid der er til midnat, og vent så længe før resten af koden kører
                var now = DateTime.Now;
                var nextMidnight = DateTime.Today.AddDays(1);
                var delay = nextMidnight - now;

                //Vent til midnat, eller indtil cancellationToken bliver cancelled, hvis API lukkes før midnat
                await Task.Delay(delay, cancellationToken);
                
                //Opret en scope for at få adgang til databasen, og nulstil alle medicins IsTaken til false
                using var scope = _serviceScopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                //Hent alle medicins fra DB og sæt IsTaken til false og gem ændringerne
                var medicins = await db.Medicins.ToListAsync(cancellationToken);
                foreach (var medicin in medicins)
                {
                    medicin.IsTaken = false;
                }
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}