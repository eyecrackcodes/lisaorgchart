using LuminaryLife.Common.Entities;
using LuminaryLife.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace LuminaryLife.Common.Data;

/// <summary>
/// Seeds the database with initial org chart data for Austin and Charlotte sites
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(CoreApiEfDbContext context)
    {
        // Check if already seeded
        if (await context.AgencySites.AnyAsync())
            return;

        // Step 1: Seed Tags
        var tags = new List<Tag>
        {
            new() { Id = 1, Name = "Training", HexColorCode = "FFA500" },
            new() { Id = 2, Name = "Performance", HexColorCode = "228B22" },
            new() { Id = 3, Name = "Tier 1", HexColorCode = "4169E1" },
            new() { Id = 4, Name = "Tier 2", HexColorCode = "9370DB" },
            new() { Id = 5, Name = "Tier 3", HexColorCode = "8B4513" },
            new() { Id = 6, Name = "Manager", HexColorCode = "2F4F4F" },
            new() { Id = 7, Name = "New Hire", HexColorCode = "FFD700" },
            new() { Id = 8, Name = "Tenured", HexColorCode = "006400" },
            new() { Id = 9, Name = "Active", HexColorCode = "32CD32" },
            new() { Id = 10, Name = "Inactive", HexColorCode = "808080" },
            new() { Id = 11, Name = "HQ", HexColorCode = "DC143C" },
            new() { Id = 12, Name = "Large Team", HexColorCode = "20B2AA" }
        };
        await context.Tags.AddRangeAsync(tags);
        await context.SaveChangesAsync();

        // Step 2: Seed Agency Sites
        var austinSite = new AgencySite
        {
            Id = 1,
            UId = "site-austin-001",
            Name = "Austin",
            City = "Austin",
            State = "TX",
            AgentCount = 45,
            TeamCount = 7,
            GoalTarget = 80
        };

        var charlotteSite = new AgencySite
        {
            Id = 2,
            UId = "site-charlotte-001",
            Name = "Charlotte",
            City = "Charlotte",
            State = "NC",
            AgentCount = 59,
            TeamCount = 8,
            GoalTarget = 70
        };

        await context.AgencySites.AddRangeAsync(austinSite, charlotteSite);
        await context.SaveChangesAsync();

        // Step 3: Seed Managers FIRST (without team assignment to avoid circular dependency)
        var managers = GetAllManagers();
        await context.Users.AddRangeAsync(managers);
        await context.SaveChangesAsync();

        // Step 4: Seed Teams (now managers exist)
        var teams = GetAllTeams();
        await context.AgencyTeams.AddRangeAsync(teams);
        await context.SaveChangesAsync();

        // Step 5: Update managers with their team assignments
        await UpdateManagerTeamAssignments(context);

        // Step 6: Seed Agents (teams now exist)
        var agents = GetAllAgents();
        await context.Users.AddRangeAsync(agents);
        await context.SaveChangesAsync();
    }

    private static List<User> GetAllManagers()
    {
        return new List<User>
        {
            // Austin Site Manager
            CreateManager("austin-site-mgr-001", "Steve Kelley", "steve.kelley@luminarylife.com", 1, null, CommissionTierEnum.Tier3, -730),
            
            // Austin Team Managers
            CreateManager("austin-mgr-druxman", "David Druxman", "david.druxman@luminarylife.com", 1, null, CommissionTierEnum.Tier2, -540),
            CreateManager("austin-mgr-holguin", "Frederick Holguin", "frederick.holguin@luminarylife.com", 1, null, CommissionTierEnum.Tier2, -600),
            CreateManager("austin-mgr-anderson", "Jay Anderson", "jay.anderson@luminarylife.com", 1, null, CommissionTierEnum.Tier2, -660),
            CreateManager("austin-mgr-mejia", "Jonathan Mejia", "jonathan.mejia@luminarylife.com", 1, null, CommissionTierEnum.Tier2, -570),
            CreateManager("austin-mgr-herrera", "Mario Herrera", "mario.herrera@luminarylife.com", 1, null, CommissionTierEnum.Tier2, -510),
            CreateManager("austin-mgr-veravillalba", "Roza Veravillalba", "roza.veravillalba@luminarylife.com", 1, null, CommissionTierEnum.Tier2, -480),
            CreateManager("austin-mgr-holts", "Jovon Holts", "jovon.holts@luminarylife.com", 1, null, CommissionTierEnum.Tier2, -360),

            // Charlotte Site Manager
            CreateManager("charlotte-site-mgr-001", "Trent Terrell", "trent.terrell@luminarylife.com", 2, null, CommissionTierEnum.Tier3, -1095),
            
            // Charlotte Team Managers
            CreateManager("charlotte-mgr-blanchett", "Vincent Blanchett", "vincent.blanchett@luminarylife.com", 2, null, CommissionTierEnum.Tier2, -600),
            CreateManager("charlotte-mgr-hajmahmoud", "Nisrin Hajmahmoud", "nisrin.hajmahmoud@luminarylife.com", 2, null, CommissionTierEnum.Tier2, -660),
            CreateManager("charlotte-mgr-espinoza", "Jovan Espinoza", "jovan.espinoza@luminarylife.com", 2, null, CommissionTierEnum.Tier2, -540),
            CreateManager("charlotte-mgr-helms", "Katelyn Helms", "katelyn.helms@luminarylife.com", 2, null, CommissionTierEnum.Tier2, -570),
            CreateManager("charlotte-mgr-fuller", "Jacob Fuller", "jacob.fuller@luminarylife.com", 2, null, CommissionTierEnum.Tier2, -510),
            CreateManager("charlotte-mgr-roman", "Miguel Roman", "miguel.roman@luminarylife.com", 2, null, CommissionTierEnum.Tier2, -480),
            CreateManager("charlotte-mgr-morgan", "Montrell Morgan", "montrell.morgan@luminarylife.com", 2, null, CommissionTierEnum.Tier2, -540),
            CreateManager("charlotte-mgr-lahti", "Brent Lahti", "brent.lahti@luminarylife.com", 2, null, CommissionTierEnum.Tier2, -450)
        };
    }

    private static List<AgencyTeam> GetAllTeams()
    {
        return new List<AgencyTeam>
        {
            // Austin Teams
            CreateTeam(1, "Team Druxman", 1, "austin-mgr-druxman", 6),
            CreateTeam(2, "Team Holguin", 1, "austin-mgr-holguin", 6),
            CreateTeam(3, "Team Anderson", 1, "austin-mgr-anderson", 9),
            CreateTeam(4, "Team Mejia", 1, "austin-mgr-mejia", 9),
            CreateTeam(5, "Team Herrera", 1, "austin-mgr-herrera", 6),
            CreateTeam(6, "Team Veravillalba", 1, "austin-mgr-veravillalba", 6),
            CreateTeam(7, "Team Holts", 1, "austin-mgr-holts", 3),

            // Charlotte Teams
            CreateTeam(8, "Team Blanchett", 2, "charlotte-mgr-blanchett", 7),
            CreateTeam(9, "Team Hajmahmoud", 2, "charlotte-mgr-hajmahmoud", 9),
            CreateTeam(10, "Team Espinoza", 2, "charlotte-mgr-espinoza", 8),
            CreateTeam(11, "Team Helms", 2, "charlotte-mgr-helms", 7),
            CreateTeam(12, "Team Fuller", 2, "charlotte-mgr-fuller", 8),
            CreateTeam(13, "Team Roman", 2, "charlotte-mgr-roman", 7),
            CreateTeam(14, "Team Morgan", 2, "charlotte-mgr-morgan", 7),
            CreateTeam(15, "Team Lahti", 2, "charlotte-mgr-lahti", 6)
        };
    }

    private static async Task UpdateManagerTeamAssignments(CoreApiEfDbContext context)
    {
        // Map manager IDs to team IDs
        var assignments = new Dictionary<string, int>
        {
            { "austin-mgr-druxman", 1 },
            { "austin-mgr-holguin", 2 },
            { "austin-mgr-anderson", 3 },
            { "austin-mgr-mejia", 4 },
            { "austin-mgr-herrera", 5 },
            { "austin-mgr-veravillalba", 6 },
            { "austin-mgr-holts", 7 },
            { "charlotte-mgr-blanchett", 8 },
            { "charlotte-mgr-hajmahmoud", 9 },
            { "charlotte-mgr-espinoza", 10 },
            { "charlotte-mgr-helms", 11 },
            { "charlotte-mgr-fuller", 12 },
            { "charlotte-mgr-roman", 13 },
            { "charlotte-mgr-morgan", 14 },
            { "charlotte-mgr-lahti", 15 }
        };

        foreach (var assignment in assignments)
        {
            var manager = await context.Users.FindAsync(assignment.Key);
            if (manager != null)
            {
                manager.AgencyTeamId = assignment.Value;
            }
        }

        await context.SaveChangesAsync();
    }

    private static List<User> GetAllAgents()
    {
        var agents = new List<User>();

        // Austin Team 1: David Druxman (6 agents)
        agents.AddRange(new[]
        {
            CreateAgent("austin-agent-001", "Eric Marrs", "eric.marrs@luminarylife.com", 1, 1, -240),
            CreateAgent("austin-agent-002", "Andrew Idahosa", "andrew.idahosa@luminarylife.com", 1, 1, -180),
            CreateAgent("austin-agent-003", "Anthony Ortiz", "anthony.ortiz@luminarylife.com", 1, 1, -300),
            CreateAgent("austin-agent-004", "Cynthia Vincent", "cynthia.vincent@luminarylife.com", 1, 1, -120),
            CreateAgent("austin-agent-005", "Doug Curttright", "doug.curttright@luminarylife.com", 1, 1, -420, CommissionTierEnum.Tier2),
            CreateAgent("austin-agent-006", "Kyle McNabb", "kyle.mcnabb@luminarylife.com", 1, 1, -90)
        });

        // Austin Team 2: Frederick Holguin (6 agents)
        agents.AddRange(new[]
        {
            CreateAgent("austin-agent-007", "Christopher Guyton", "christopher.guyton@luminarylife.com", 1, 2, -210),
            CreateAgent("austin-agent-008", "Diane Gauthier", "diane.gauthier@luminarylife.com", 1, 2, -270),
            CreateAgent("austin-agent-009", "Jermeigh Jones", "jermeigh.jones@luminarylife.com", 1, 2, -150),
            CreateAgent("austin-agent-010", "John Smith", "john.smith@luminarylife.com", 1, 2, -480, CommissionTierEnum.Tier2),
            CreateAgent("austin-agent-011", "Miguel Palacios", "miguel.palacios@luminarylife.com", 1, 2, -120),
            CreateAgent("austin-agent-012", "Timothy Galmore", "timothy.galmore@luminarylife.com", 1, 2, -180)
        });

        // Austin Team 3: Jay Anderson (9 agents)
        agents.AddRange(new[]
        {
            CreateAgent("austin-agent-013", "Brando Pina", "brando.pina@luminarylife.com", 1, 3, -240),
            CreateAgent("austin-agent-014", "Ernest Salazar", "ernest.salazar@luminarylife.com", 1, 3, -330),
            CreateAgent("austin-agent-015", "Krystian Galmore", "krystian.galmore@luminarylife.com", 1, 3, -180),
            CreateAgent("austin-agent-016", "Leslie Chandler", "leslie.chandler@luminarylife.com", 1, 3, -450, CommissionTierEnum.Tier2),
            CreateAgent("austin-agent-017", "Mark Kaufman", "mark.kaufman@luminarylife.com", 1, 3, -150),
            CreateAgent("austin-agent-018", "Noah Nunn", "noah.nunn@luminarylife.com", 1, 3, -210),
            CreateAgent("austin-agent-019", "Noah Wimberly", "noah.wimberly@luminarylife.com", 1, 3, -120),
            CreateAgent("austin-agent-020", "Uvaldo Acosta", "uvaldo.acosta@luminarylife.com", 1, 3, -270),
            CreateAgent("austin-agent-021", "Veronica Aleman", "veronica.aleman@luminarylife.com", 1, 3, -90)
        });

        // Austin Team 4: Jonathan Mejia (9 agents)
        agents.AddRange(new[]
        {
            CreateAgent("austin-agent-022", "Brandon Simmons", "brandon.simmons@luminarylife.com", 1, 4, -300),
            CreateAgent("austin-agent-023", "Alex Walters", "alex.walters@luminarylife.com", 1, 4, -240),
            CreateAgent("austin-agent-024", "Angelo Baca", "angelo.baca@luminarylife.com", 1, 4, -180),
            CreateAgent("austin-agent-025", "Austin Houser", "austin.houser@luminarylife.com", 1, 4, -150),
            CreateAgent("austin-agent-026", "Brian Ullestad", "brian.ullestad@luminarylife.com", 1, 4, -420, CommissionTierEnum.Tier2),
            CreateAgent("austin-agent-027", "Chris Cantu", "chris.cantu@luminarylife.com", 1, 4, -210),
            CreateAgent("austin-agent-028", "David Escamilla", "david.escamilla@luminarylife.com", 1, 4, -60, CommissionTierEnum.None, AgentTypeEnum.Training),
            CreateAgent("austin-agent-029", "Jamal Washington", "jamal.washington@luminarylife.com", 1, 4, -120),
            CreateAgent("austin-agent-030", "Jonathon Dubbs", "jonathon.dubbs@luminarylife.com", 1, 4, -90)
        });

        // Austin Team 5: Mario Herrera (6 agents)
        agents.AddRange(new[]
        {
            CreateAgent("austin-agent-031", "Alisha O'Bryant", "alisha.obryant@luminarylife.com", 1, 5, -270),
            CreateAgent("austin-agent-032", "AD Hutton", "ad.hutton@luminarylife.com", 1, 5, -210),
            CreateAgent("austin-agent-033", "Bob Gallagher", "bob.gallagher@luminarylife.com", 1, 5, -390, CommissionTierEnum.Tier2),
            CreateAgent("austin-agent-034", "Jamila Campbell", "jamila.campbell@luminarylife.com", 1, 5, -150),
            CreateAgent("austin-agent-035", "Nancy Hunter", "nancy.hunter@luminarylife.com", 1, 5, -180),
            CreateAgent("austin-agent-036", "Steve Brown", "steve.brown@luminarylife.com", 1, 5, -120)
        });

        // Austin Team 6: Roza Veravillalba (6 agents)
        agents.AddRange(new[]
        {
            CreateAgent("austin-agent-037", "John Sivy", "john.sivy@luminarylife.com", 1, 6, -240),
            CreateAgent("austin-agent-038", "Aaron Mason", "aaron.mason@luminarylife.com", 1, 6, -180),
            CreateAgent("austin-agent-039", "Kameron Dollar", "kameron.dollar@luminarylife.com", 1, 6, -150),
            CreateAgent("austin-agent-040", "Matthew Reyes", "matthew.reyes@luminarylife.com", 1, 6, -210),
            CreateAgent("austin-agent-041", "Nathan Acker", "nathan.acker@luminarylife.com", 1, 6, -120),
            CreateAgent("austin-agent-042", "Vania Rodriguez", "vania.rodriguez@luminarylife.com", 1, 6, -90)
        });

        // Austin Team 7: Jovon Holts (3 agents)
        agents.AddRange(new[]
        {
            CreateAgent("austin-agent-043", "Damion Silveria", "damion.silveria@luminarylife.com", 1, 7, -150),
            CreateAgent("austin-agent-044", "Jimmy Hoang", "jimmy.hoang@luminarylife.com", 1, 7, -120),
            CreateAgent("austin-agent-045", "KC Kang", "kc.kang@luminarylife.com", 1, 7, -90)
        });

        // Charlotte Team 8: Vincent Blanchett (7 agents)
        agents.AddRange(new[]
        {
            CreateAgent("charlotte-agent-001", "Lynethe Guevara", "lynethe.guevara@luminarylife.com", 2, 8, -270),
            CreateAgent("charlotte-agent-002", "Adelina Guardado", "adelina.guardado@luminarylife.com", 2, 8, -210),
            CreateAgent("charlotte-agent-003", "Doug Yang", "doug.yang@luminarylife.com", 2, 8, -180),
            CreateAgent("charlotte-agent-004", "Mitchell Pittman", "mitchell.pittman@luminarylife.com", 2, 8, -420, CommissionTierEnum.Tier2),
            CreateAgent("charlotte-agent-005", "Tahveon Washington", "tahveon.washington@luminarylife.com", 2, 8, -150),
            CreateAgent("charlotte-agent-006", "Jada Jones", "jada.jones@luminarylife.com", 2, 8, -120),
            CreateAgent("charlotte-agent-007", "Maurice Grier", "maurice.grier@luminarylife.com", 2, 8, -90)
        });

        // Charlotte Team 9: Nisrin Hajmahmoud (9 agents)
        agents.AddRange(new[]
        {
            CreateAgent("charlotte-agent-008", "Serena Cowan", "serena.cowan@luminarylife.com", 2, 9, -300),
            CreateAgent("charlotte-agent-009", "Chris Chen", "chris.chen@luminarylife.com", 2, 9, -480, CommissionTierEnum.Tier2),
            CreateAgent("charlotte-agent-010", "Jarrelle Williams", "jarrelle.williams@luminarylife.com", 2, 9, -240),
            CreateAgent("charlotte-agent-011", "Chester Hannah", "chester.hannah@luminarylife.com", 2, 9, -210),
            CreateAgent("charlotte-agent-012", "Terrial Buycks", "terrial.buycks@luminarylife.com", 2, 9, -180),
            CreateAgent("charlotte-agent-013", "Ciarra Cave", "ciarra.cave@luminarylife.com", 2, 9, -150),
            CreateAgent("charlotte-agent-014", "Dawn Strong", "dawn.strong@luminarylife.com", 2, 9, -120),
            CreateAgent("charlotte-agent-015", "Lamont Perry", "lamont.perry@luminarylife.com", 2, 9, -270),
            CreateAgent("charlotte-agent-016", "Jerren Cropps", "jerren.cropps@luminarylife.com", 2, 9, -90)
        });

        // Charlotte Team 10: Jovan Espinoza (8 agents)
        agents.AddRange(new[]
        {
            CreateAgent("charlotte-agent-017", "Lasondra Davis", "lasondra.davis@luminarylife.com", 2, 10, -270),
            CreateAgent("charlotte-agent-018", "Alana Tanksley", "alana.tanksley@luminarylife.com", 2, 10, -210),
            CreateAgent("charlotte-agent-019", "Matt Buffington", "matt.buffington@luminarylife.com", 2, 10, -450, CommissionTierEnum.Tier2),
            CreateAgent("charlotte-agent-020", "Sean Leary", "sean.leary@luminarylife.com", 2, 10, -180),
            CreateAgent("charlotte-agent-021", "Troy Wigfall", "troy.wigfall@luminarylife.com", 2, 10, -150),
            CreateAgent("charlotte-agent-022", "Russell Tvedt", "russell.tvedt@luminarylife.com", 2, 10, -120),
            CreateAgent("charlotte-agent-023", "Gil Matamoros", "gil.matamoros@luminarylife.com", 2, 10, -240),
            CreateAgent("charlotte-agent-024", "Loren Johnson", "loren.johnson@luminarylife.com", 2, 10, -90)
        });

        // Charlotte Team 11: Katelyn Helms (7 agents)
        agents.AddRange(new[]
        {
            CreateAgent("charlotte-agent-025", "Beau Carson", "beau.carson@luminarylife.com", 2, 11, -300),
            CreateAgent("charlotte-agent-026", "Kyle Williford", "kyle.williford@luminarylife.com", 2, 11, -240),
            CreateAgent("charlotte-agent-027", "Don McCoy", "don.mccoy@luminarylife.com", 2, 11, -420, CommissionTierEnum.Tier2),
            CreateAgent("charlotte-agent-028", "Bryon Griffin", "bryon.griffin@luminarylife.com", 2, 11, -180),
            CreateAgent("charlotte-agent-029", "Barion Jones", "barion.jones@luminarylife.com", 2, 11, -150),
            CreateAgent("charlotte-agent-030", "James Batton", "james.batton@luminarylife.com", 2, 11, -120),
            CreateAgent("charlotte-agent-031", "Ockeyana Williams", "ockeyana.williams@luminarylife.com", 2, 11, -210)
        });

        // Charlotte Team 12: Jacob Fuller (8 agents)
        agents.AddRange(new[]
        {
            CreateAgent("charlotte-agent-032", "Jeff Rosenberg", "jeff.rosenberg@luminarylife.com", 2, 12, -270),
            CreateAgent("charlotte-agent-033", "Chris Williams", "chris.williams@luminarylife.com", 2, 12, -210),
            CreateAgent("charlotte-agent-034", "David Thompson", "david.thompson@luminarylife.com", 2, 12, -390, CommissionTierEnum.Tier2),
            CreateAgent("charlotte-agent-035", "Gavin Stacks", "gavin.stacks@luminarylife.com", 2, 12, -180),
            CreateAgent("charlotte-agent-036", "Divearl Brown", "divearl.brown@luminarylife.com", 2, 12, -150),
            CreateAgent("charlotte-agent-037", "Shannon Little", "shannon.little@luminarylife.com", 2, 12, -120),
            CreateAgent("charlotte-agent-038", "Terrence Robinson", "terrence.robinson@luminarylife.com", 2, 12, -240),
            CreateAgent("charlotte-agent-039", "Alvin Fulmore", "alvin.fulmore@luminarylife.com", 2, 12, -90)
        });

        // Charlotte Team 13: Miguel Roman (7 agents)
        agents.AddRange(new[]
        {
            CreateAgent("charlotte-agent-040", "Kevin Gilliard", "kevin.gilliard@luminarylife.com", 2, 13, -300),
            CreateAgent("charlotte-agent-041", "Tamara Hemmings", "tamara.hemmings@luminarylife.com", 2, 13, -240),
            CreateAgent("charlotte-agent-042", "Phillip Mcewen", "phillip.mcewen@luminarylife.com", 2, 13, -450, CommissionTierEnum.Tier2),
            CreateAgent("charlotte-agent-043", "Tiffany Holliman", "tiffany.holliman@luminarylife.com", 2, 13, -180),
            CreateAgent("charlotte-agent-044", "Jaime Umanzor", "jaime.umanzor@luminarylife.com", 2, 13, -150),
            CreateAgent("charlotte-agent-045", "Traci Drummond", "traci.drummond@luminarylife.com", 2, 13, -120),
            CreateAgent("charlotte-agent-046", "Gregory Powell", "gregory.powell@luminarylife.com", 2, 13, -210)
        });

        // Charlotte Team 14: Montrell Morgan (7 agents)
        agents.AddRange(new[]
        {
            CreateAgent("charlotte-agent-047", "Jimmie Royster IV", "jimmie.royster@luminarylife.com", 2, 14, -270),
            CreateAgent("charlotte-agent-048", "Michael Masuck", "michael.masuck@luminarylife.com", 2, 14, -210),
            CreateAgent("charlotte-agent-049", "Chad Gammons", "chad.gammons@luminarylife.com", 2, 14, -420, CommissionTierEnum.Tier2),
            CreateAgent("charlotte-agent-050", "Robert Mulligan", "robert.mulligan@luminarylife.com", 2, 14, -180),
            CreateAgent("charlotte-agent-051", "Jabari McKnight", "jabari.mcknight@luminarylife.com", 2, 14, -150),
            CreateAgent("charlotte-agent-052", "Naimah German", "naimah.german@luminarylife.com", 2, 14, -120),
            CreateAgent("charlotte-agent-053", "Caleb McIntosh", "caleb.mcintosh@luminarylife.com", 2, 14, -240)
        });

        // Charlotte Team 15: Brent Lahti (6 agents)
        agents.AddRange(new[]
        {
            CreateAgent("charlotte-agent-054", "Quanikko Fernandors", "quanikko.fernandors@luminarylife.com", 2, 15, -300),
            CreateAgent("charlotte-agent-055", "Derrick Horne", "derrick.horne@luminarylife.com", 2, 15, -240),
            CreateAgent("charlotte-agent-056", "Samantha Gentle", "samantha.gentle@luminarylife.com", 2, 15, -180),
            CreateAgent("charlotte-agent-057", "Celina Tuck", "celina.tuck@luminarylife.com", 2, 15, -150),
            CreateAgent("charlotte-agent-058", "Terrell Dillard", "terrell.dillard@luminarylife.com", 2, 15, -120),
            CreateAgent("charlotte-agent-059", "David Bedington", "david.bedington@luminarylife.com", 2, 15, -90)
        });

        return agents;
    }

    private static User CreateManager(string id, string name, string email, int siteId, int? teamId, CommissionTierEnum tier, int daysAgo)
    {
        return new User
        {
            Id = id,
            Name = name,
            Email = email,
            AgencySiteId = siteId,
            AgencyTeamId = teamId,
            Title = TitleEnum.Manager,
            AgentStatus = AgentStatusEnum.Active,
            AgentType = AgentTypeEnum.Performance,
            CommissionTier = tier,
            AgencyStartDate = DateTime.UtcNow.AddDays(daysAgo)
        };
    }

    private static User CreateAgent(
        string id,
        string name,
        string email,
        int siteId,
        int teamId,
        int daysAgo,
        CommissionTierEnum tier = CommissionTierEnum.Tier1,
        AgentTypeEnum agentType = AgentTypeEnum.Performance)
    {
        return new User
        {
            Id = id,
            Name = name,
            Email = email,
            AgencySiteId = siteId,
            AgencyTeamId = teamId,
            Title = TitleEnum.Agent,
            AgentStatus = AgentStatusEnum.Active,
            AgentType = agentType,
            CommissionTier = tier,
            AgencyStartDate = DateTime.UtcNow.AddDays(daysAgo)
        };
    }

    private static AgencyTeam CreateTeam(int id, string name, int siteId, string managerId, int memberCount)
    {
        return new AgencyTeam
        {
            Id = id,
            UId = $"team-{id:D3}",
            Name = name,
            AgencySiteId = siteId,
            ManagerUserId = managerId,
            MemberCount = memberCount
        };
    }
}
