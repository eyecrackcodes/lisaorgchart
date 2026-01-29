-- LuminaryLife Org Chart Seed Data
-- Austin Site: 45 agents (7 teams)
-- Charlotte Site: 59 agents (8 teams)

-- ============================================
-- TAGS
-- ============================================
INSERT INTO tags (Id, Name, HexColorCode, CreatedAt) VALUES
(1, 'Training', 'FFA500', datetime('now')),
(2, 'Performance', '228B22', datetime('now')),
(3, 'Tier 1', '4169E1', datetime('now')),
(4, 'Tier 2', '9370DB', datetime('now')),
(5, 'Tier 3', '8B4513', datetime('now')),
(6, 'Manager', '2F4F4F', datetime('now')),
(7, 'New Hire', 'FFD700', datetime('now')),
(8, 'Tenured', '006400', datetime('now')),
(9, 'Active', '32CD32', datetime('now')),
(10, 'Inactive', '808080', datetime('now')),
(11, 'HQ', 'DC143C', datetime('now')),
(12, 'Large Team', '20B2AA', datetime('now'));

-- ============================================
-- AGENCY SITES
-- ============================================
INSERT INTO agency_sites (Id, UId, Name, City, State, AgentCount, TeamCount, GoalTarget, CreatedAt) VALUES
(1, 'site-austin-001', 'Austin', 'Austin', 'TX', 45, 7, 80, datetime('now')),
(2, 'site-charlotte-001', 'Charlotte', 'Charlotte', 'NC', 59, 8, 70, datetime('now'));

-- ============================================
-- AUSTIN TEAMS (Site ID = 1)
-- ============================================
-- Site Manager: Steve Kelley
INSERT INTO users (Id, Name, Email, AgencySiteId, AgencyTeamId, Title, AgentStatus, AgentType, CommissionTier, AgencyStartDate, CreatedAt) VALUES
('austin-site-mgr-001', 'Steve Kelley', 'steve.kelley@luminarylife.com', 1, NULL, 2, 1, 2, 3, datetime('now', '-2 years'), datetime('now'));

-- Team 1: David Druxman (6 agents)
INSERT INTO agency_teams (Id, UId, Name, AgencySiteId, ManagerUserId, MemberCount, CreatedAt) VALUES
(1, 'team-druxman-001', 'Team Druxman', 1, 'austin-mgr-druxman', 6, datetime('now'));

INSERT INTO users (Id, Name, Email, AgencySiteId, AgencyTeamId, Title, AgentStatus, AgentType, CommissionTier, AgencyStartDate, CreatedAt) VALUES
('austin-mgr-druxman', 'David Druxman', 'david.druxman@luminarylife.com', 1, 1, 2, 1, 2, 2, datetime('now', '-18 months'), datetime('now')),
('austin-agent-001', 'Eric Marrs', 'eric.marrs@luminarylife.com', 1, 1, 1, 1, 2, 1, datetime('now', '-8 months'), datetime('now')),
('austin-agent-002', 'Andrew Idahosa', 'andrew.idahosa@luminarylife.com', 1, 1, 1, 1, 2, 1, datetime('now', '-6 months'), datetime('now')),
('austin-agent-003', 'Anthony Ortiz', 'anthony.ortiz@luminarylife.com', 1, 1, 1, 1, 2, 1, datetime('now', '-10 months'), datetime('now')),
('austin-agent-004', 'Cynthia Vincent', 'cynthia.vincent@luminarylife.com', 1, 1, 1, 1, 2, 1, datetime('now', '-4 months'), datetime('now')),
('austin-agent-005', 'Doug Curttright', 'doug.curttright@luminarylife.com', 1, 1, 1, 1, 2, 2, datetime('now', '-14 months'), datetime('now')),
('austin-agent-006', 'Kyle McNabb', 'kyle.mcnabb@luminarylife.com', 1, 1, 1, 1, 2, 1, datetime('now', '-3 months'), datetime('now'));

-- Team 2: Frederick Holguin (6 agents)
INSERT INTO agency_teams (Id, UId, Name, AgencySiteId, ManagerUserId, MemberCount, CreatedAt) VALUES
(2, 'team-holguin-001', 'Team Holguin', 1, 'austin-mgr-holguin', 6, datetime('now'));

INSERT INTO users (Id, Name, Email, AgencySiteId, AgencyTeamId, Title, AgentStatus, AgentType, CommissionTier, AgencyStartDate, CreatedAt) VALUES
('austin-mgr-holguin', 'Frederick Holguin', 'frederick.holguin@luminarylife.com', 1, 2, 2, 1, 2, 2, datetime('now', '-20 months'), datetime('now')),
('austin-agent-007', 'Christopher Guyton', 'christopher.guyton@luminarylife.com', 1, 2, 1, 1, 2, 1, datetime('now', '-7 months'), datetime('now')),
('austin-agent-008', 'Diane Gauthier', 'diane.gauthier@luminarylife.com', 1, 2, 1, 1, 2, 1, datetime('now', '-9 months'), datetime('now')),
('austin-agent-009', 'Jermeigh Jones', 'jermeigh.jones@luminarylife.com', 1, 2, 1, 1, 2, 1, datetime('now', '-5 months'), datetime('now')),
('austin-agent-010', 'John Smith', 'john.smith@luminarylife.com', 1, 2, 1, 1, 2, 2, datetime('now', '-16 months'), datetime('now')),
('austin-agent-011', 'Miguel Palacios', 'miguel.palacios@luminarylife.com', 1, 2, 1, 1, 2, 1, datetime('now', '-4 months'), datetime('now')),
('austin-agent-012', 'Timothy Galmore', 'timothy.galmore@luminarylife.com', 1, 2, 1, 1, 2, 1, datetime('now', '-6 months'), datetime('now'));

-- Team 3: Jay Anderson (9 agents)
INSERT INTO agency_teams (Id, UId, Name, AgencySiteId, ManagerUserId, MemberCount, CreatedAt) VALUES
(3, 'team-anderson-001', 'Team Anderson', 1, 'austin-mgr-anderson', 9, datetime('now'));

INSERT INTO users (Id, Name, Email, AgencySiteId, AgencyTeamId, Title, AgentStatus, AgentType, CommissionTier, AgencyStartDate, CreatedAt) VALUES
('austin-mgr-anderson', 'Jay Anderson', 'jay.anderson@luminarylife.com', 1, 3, 2, 1, 2, 2, datetime('now', '-22 months'), datetime('now')),
('austin-agent-013', 'Brando Pina', 'brando.pina@luminarylife.com', 1, 3, 1, 1, 2, 1, datetime('now', '-8 months'), datetime('now')),
('austin-agent-014', 'Ernest Salazar', 'ernest.salazar@luminarylife.com', 1, 3, 1, 1, 2, 1, datetime('now', '-11 months'), datetime('now')),
('austin-agent-015', 'Krystian Galmore', 'krystian.galmore@luminarylife.com', 1, 3, 1, 1, 2, 1, datetime('now', '-6 months'), datetime('now')),
('austin-agent-016', 'Leslie Chandler', 'leslie.chandler@luminarylife.com', 1, 3, 1, 1, 2, 2, datetime('now', '-15 months'), datetime('now')),
('austin-agent-017', 'Mark Kaufman', 'mark.kaufman@luminarylife.com', 1, 3, 1, 1, 2, 1, datetime('now', '-5 months'), datetime('now')),
('austin-agent-018', 'Noah Nunn', 'noah.nunn@luminarylife.com', 1, 3, 1, 1, 2, 1, datetime('now', '-7 months'), datetime('now')),
('austin-agent-019', 'Noah Wimberly', 'noah.wimberly@luminarylife.com', 1, 3, 1, 1, 2, 1, datetime('now', '-4 months'), datetime('now')),
('austin-agent-020', 'Uvaldo Acosta', 'uvaldo.acosta@luminarylife.com', 1, 3, 1, 1, 2, 1, datetime('now', '-9 months'), datetime('now')),
('austin-agent-021', 'Veronica Aleman', 'veronica.aleman@luminarylife.com', 1, 3, 1, 1, 2, 1, datetime('now', '-3 months'), datetime('now'));

-- Team 4: Jonathan Mejia (9 agents)
INSERT INTO agency_teams (Id, UId, Name, AgencySiteId, ManagerUserId, MemberCount, CreatedAt) VALUES
(4, 'team-mejia-001', 'Team Mejia', 1, 'austin-mgr-mejia', 9, datetime('now'));

INSERT INTO users (Id, Name, Email, AgencySiteId, AgencyTeamId, Title, AgentStatus, AgentType, CommissionTier, AgencyStartDate, CreatedAt) VALUES
('austin-mgr-mejia', 'Jonathan Mejia', 'jonathan.mejia@luminarylife.com', 1, 4, 2, 1, 2, 2, datetime('now', '-19 months'), datetime('now')),
('austin-agent-022', 'Brandon Simmons', 'brandon.simmons@luminarylife.com', 1, 4, 1, 1, 2, 1, datetime('now', '-10 months'), datetime('now')),
('austin-agent-023', 'Alex Walters', 'alex.walters@luminarylife.com', 1, 4, 1, 1, 2, 1, datetime('now', '-8 months'), datetime('now')),
('austin-agent-024', 'Angelo Baca', 'angelo.baca@luminarylife.com', 1, 4, 1, 1, 2, 1, datetime('now', '-6 months'), datetime('now')),
('austin-agent-025', 'Austin Houser', 'austin.houser@luminarylife.com', 1, 4, 1, 1, 2, 1, datetime('now', '-5 months'), datetime('now')),
('austin-agent-026', 'Brian Ullestad', 'brian.ullestad@luminarylife.com', 1, 4, 1, 1, 2, 2, datetime('now', '-14 months'), datetime('now')),
('austin-agent-027', 'Chris Cantu', 'chris.cantu@luminarylife.com', 1, 4, 1, 1, 2, 1, datetime('now', '-7 months'), datetime('now')),
('austin-agent-028', 'David Escamilla', 'david.escamilla@luminarylife.com', 1, 4, 1, 1, 1, 0, datetime('now', '-2 months'), datetime('now')),
('austin-agent-029', 'Jamal Washington', 'jamal.washington@luminarylife.com', 1, 4, 1, 1, 2, 1, datetime('now', '-4 months'), datetime('now')),
('austin-agent-030', 'Jonathon Dubbs', 'jonathon.dubbs@luminarylife.com', 1, 4, 1, 1, 2, 1, datetime('now', '-3 months'), datetime('now'));

-- Team 5: Mario Herrera (6 agents)
INSERT INTO agency_teams (Id, UId, Name, AgencySiteId, ManagerUserId, MemberCount, CreatedAt) VALUES
(5, 'team-herrera-001', 'Team Herrera', 1, 'austin-mgr-herrera', 6, datetime('now'));

INSERT INTO users (Id, Name, Email, AgencySiteId, AgencyTeamId, Title, AgentStatus, AgentType, CommissionTier, AgencyStartDate, CreatedAt) VALUES
('austin-mgr-herrera', 'Mario Herrera', 'mario.herrera@luminarylife.com', 1, 5, 2, 1, 2, 2, datetime('now', '-17 months'), datetime('now')),
('austin-agent-031', 'Alisha O''Bryant', 'alisha.obryant@luminarylife.com', 1, 5, 1, 1, 2, 1, datetime('now', '-9 months'), datetime('now')),
('austin-agent-032', 'AD Hutton', 'ad.hutton@luminarylife.com', 1, 5, 1, 1, 2, 1, datetime('now', '-7 months'), datetime('now')),
('austin-agent-033', 'Bob Gallagher', 'bob.gallagher@luminarylife.com', 1, 5, 1, 1, 2, 2, datetime('now', '-13 months'), datetime('now')),
('austin-agent-034', 'Jamila Campbell', 'jamila.campbell@luminarylife.com', 1, 5, 1, 1, 2, 1, datetime('now', '-5 months'), datetime('now')),
('austin-agent-035', 'Nancy Hunter', 'nancy.hunter@luminarylife.com', 1, 5, 1, 1, 2, 1, datetime('now', '-6 months'), datetime('now')),
('austin-agent-036', 'Steve Brown', 'steve.brown@luminarylife.com', 1, 5, 1, 1, 2, 1, datetime('now', '-4 months'), datetime('now'));

-- Team 6: Roza Veravillalba (6 agents)
INSERT INTO agency_teams (Id, UId, Name, AgencySiteId, ManagerUserId, MemberCount, CreatedAt) VALUES
(6, 'team-veravillalba-001', 'Team Veravillalba', 1, 'austin-mgr-veravillalba', 6, datetime('now'));

INSERT INTO users (Id, Name, Email, AgencySiteId, AgencyTeamId, Title, AgentStatus, AgentType, CommissionTier, AgencyStartDate, CreatedAt) VALUES
('austin-mgr-veravillalba', 'Roza Veravillalba', 'roza.veravillalba@luminarylife.com', 1, 6, 2, 1, 2, 2, datetime('now', '-16 months'), datetime('now')),
('austin-agent-037', 'John Sivy', 'john.sivy@luminarylife.com', 1, 6, 1, 1, 2, 1, datetime('now', '-8 months'), datetime('now')),
('austin-agent-038', 'Aaron Mason', 'aaron.mason@luminarylife.com', 1, 6, 1, 1, 2, 1, datetime('now', '-6 months'), datetime('now')),
('austin-agent-039', 'Kameron Dollar', 'kameron.dollar@luminarylife.com', 1, 6, 1, 1, 2, 1, datetime('now', '-5 months'), datetime('now')),
('austin-agent-040', 'Matthew Reyes', 'matthew.reyes@luminarylife.com', 1, 6, 1, 1, 2, 1, datetime('now', '-7 months'), datetime('now')),
('austin-agent-041', 'Nathan Acker', 'nathan.acker@luminarylife.com', 1, 6, 1, 1, 2, 1, datetime('now', '-4 months'), datetime('now')),
('austin-agent-042', 'Vania Rodriguez', 'vania.rodriguez@luminarylife.com', 1, 6, 1, 1, 2, 1, datetime('now', '-3 months'), datetime('now'));

-- Team 7: Jovon Holts (3 agents)
INSERT INTO agency_teams (Id, UId, Name, AgencySiteId, ManagerUserId, MemberCount, CreatedAt) VALUES
(7, 'team-holts-001', 'Team Holts', 1, 'austin-mgr-holts', 3, datetime('now'));

INSERT INTO users (Id, Name, Email, AgencySiteId, AgencyTeamId, Title, AgentStatus, AgentType, CommissionTier, AgencyStartDate, CreatedAt) VALUES
('austin-mgr-holts', 'Jovon Holts', 'jovon.holts@luminarylife.com', 1, 7, 2, 1, 2, 2, datetime('now', '-12 months'), datetime('now')),
('austin-agent-043', 'Damion Silveria', 'damion.silveria@luminarylife.com', 1, 7, 1, 1, 2, 1, datetime('now', '-5 months'), datetime('now')),
('austin-agent-044', 'Jimmy Hoang', 'jimmy.hoang@luminarylife.com', 1, 7, 1, 1, 2, 1, datetime('now', '-4 months'), datetime('now')),
('austin-agent-045', 'KC Kang', 'kc.kang@luminarylife.com', 1, 7, 1, 1, 2, 1, datetime('now', '-3 months'), datetime('now'));

-- ============================================
-- CHARLOTTE TEAMS (Site ID = 2)
-- ============================================
-- Site Manager: Trent Terrell
INSERT INTO users (Id, Name, Email, AgencySiteId, AgencyTeamId, Title, AgentStatus, AgentType, CommissionTier, AgencyStartDate, CreatedAt) VALUES
('charlotte-site-mgr-001', 'Trent Terrell', 'trent.terrell@luminarylife.com', 2, NULL, 2, 1, 2, 3, datetime('now', '-3 years'), datetime('now'));

-- Team 8: Vincent Blanchett (7 agents)
INSERT INTO agency_teams (Id, UId, Name, AgencySiteId, ManagerUserId, MemberCount, CreatedAt) VALUES
(8, 'team-blanchett-001', 'Team Blanchett', 2, 'charlotte-mgr-blanchett', 7, datetime('now'));

INSERT INTO users (Id, Name, Email, AgencySiteId, AgencyTeamId, Title, AgentStatus, AgentType, CommissionTier, AgencyStartDate, CreatedAt) VALUES
('charlotte-mgr-blanchett', 'Vincent Blanchett', 'vincent.blanchett@luminarylife.com', 2, 8, 2, 1, 2, 2, datetime('now', '-20 months'), datetime('now')),
('charlotte-agent-001', 'Lynethe Guevara', 'lynethe.guevara@luminarylife.com', 2, 8, 1, 1, 2, 1, datetime('now', '-9 months'), datetime('now')),
('charlotte-agent-002', 'Adelina Guardado', 'adelina.guardado@luminarylife.com', 2, 8, 1, 1, 2, 1, datetime('now', '-7 months'), datetime('now')),
('charlotte-agent-003', 'Doug Yang', 'doug.yang@luminarylife.com', 2, 8, 1, 1, 2, 1, datetime('now', '-6 months'), datetime('now')),
('charlotte-agent-004', 'Mitchell Pittman', 'mitchell.pittman@luminarylife.com', 2, 8, 1, 1, 2, 2, datetime('now', '-14 months'), datetime('now')),
('charlotte-agent-005', 'Tahveon Washington', 'tahveon.washington@luminarylife.com', 2, 8, 1, 1, 2, 1, datetime('now', '-5 months'), datetime('now')),
('charlotte-agent-006', 'Jada Jones', 'jada.jones@luminarylife.com', 2, 8, 1, 1, 2, 1, datetime('now', '-4 months'), datetime('now')),
('charlotte-agent-007', 'Maurice Grier', 'maurice.grier@luminarylife.com', 2, 8, 1, 1, 2, 1, datetime('now', '-3 months'), datetime('now'));

-- Team 9: Nisrin Hajmahmoud (9 agents)
INSERT INTO agency_teams (Id, UId, Name, AgencySiteId, ManagerUserId, MemberCount, CreatedAt) VALUES
(9, 'team-hajmahmoud-001', 'Team Hajmahmoud', 2, 'charlotte-mgr-hajmahmoud', 9, datetime('now'));

INSERT INTO users (Id, Name, Email, AgencySiteId, AgencyTeamId, Title, AgentStatus, AgentType, CommissionTier, AgencyStartDate, CreatedAt) VALUES
('charlotte-mgr-hajmahmoud', 'Nisrin Hajmahmoud', 'nisrin.hajmahmoud@luminarylife.com', 2, 9, 2, 1, 2, 2, datetime('now', '-22 months'), datetime('now')),
('charlotte-agent-008', 'Serena Cowan', 'serena.cowan@luminarylife.com', 2, 9, 1, 1, 2, 1, datetime('now', '-10 months'), datetime('now')),
('charlotte-agent-009', 'Chris Chen', 'chris.chen@luminarylife.com', 2, 9, 1, 1, 2, 2, datetime('now', '-16 months'), datetime('now')),
('charlotte-agent-010', 'Jarrelle Williams', 'jarrelle.williams@luminarylife.com', 2, 9, 1, 1, 2, 1, datetime('now', '-8 months'), datetime('now')),
('charlotte-agent-011', 'Chester Hannah', 'chester.hannah@luminarylife.com', 2, 9, 1, 1, 2, 1, datetime('now', '-7 months'), datetime('now')),
('charlotte-agent-012', 'Terrial Buycks', 'terrial.buycks@luminarylife.com', 2, 9, 1, 1, 2, 1, datetime('now', '-6 months'), datetime('now')),
('charlotte-agent-013', 'Ciarra Cave', 'ciarra.cave@luminarylife.com', 2, 9, 1, 1, 2, 1, datetime('now', '-5 months'), datetime('now')),
('charlotte-agent-014', 'Dawn Strong', 'dawn.strong@luminarylife.com', 2, 9, 1, 1, 2, 1, datetime('now', '-4 months'), datetime('now')),
('charlotte-agent-015', 'Lamont Perry', 'lamont.perry@luminarylife.com', 2, 9, 1, 1, 2, 1, datetime('now', '-9 months'), datetime('now')),
('charlotte-agent-016', 'Jerren Cropps', 'jerren.cropps@luminarylife.com', 2, 9, 1, 1, 2, 1, datetime('now', '-3 months'), datetime('now'));

-- Team 10: Jovan Espinoza (8 agents)
INSERT INTO agency_teams (Id, UId, Name, AgencySiteId, ManagerUserId, MemberCount, CreatedAt) VALUES
(10, 'team-espinoza-001', 'Team Espinoza', 2, 'charlotte-mgr-espinoza', 8, datetime('now'));

INSERT INTO users (Id, Name, Email, AgencySiteId, AgencyTeamId, Title, AgentStatus, AgentType, CommissionTier, AgencyStartDate, CreatedAt) VALUES
('charlotte-mgr-espinoza', 'Jovan Espinoza', 'jovan.espinoza@luminarylife.com', 2, 10, 2, 1, 2, 2, datetime('now', '-18 months'), datetime('now')),
('charlotte-agent-017', 'Lasondra Davis', 'lasondra.davis@luminarylife.com', 2, 10, 1, 1, 2, 1, datetime('now', '-9 months'), datetime('now')),
('charlotte-agent-018', 'Alana Tanksley', 'alana.tanksley@luminarylife.com', 2, 10, 1, 1, 2, 1, datetime('now', '-7 months'), datetime('now')),
('charlotte-agent-019', 'Matt Buffington', 'matt.buffington@luminarylife.com', 2, 10, 1, 1, 2, 2, datetime('now', '-15 months'), datetime('now')),
('charlotte-agent-020', 'Sean Leary', 'sean.leary@luminarylife.com', 2, 10, 1, 1, 2, 1, datetime('now', '-6 months'), datetime('now')),
('charlotte-agent-021', 'Troy Wigfall', 'troy.wigfall@luminarylife.com', 2, 10, 1, 1, 2, 1, datetime('now', '-5 months'), datetime('now')),
('charlotte-agent-022', 'Russell Tvedt', 'russell.tvedt@luminarylife.com', 2, 10, 1, 1, 2, 1, datetime('now', '-4 months'), datetime('now')),
('charlotte-agent-023', 'Gil Matamoros', 'gil.matamoros@luminarylife.com', 2, 10, 1, 1, 2, 1, datetime('now', '-8 months'), datetime('now')),
('charlotte-agent-024', 'Loren Johnson', 'loren.johnson@luminarylife.com', 2, 10, 1, 1, 2, 1, datetime('now', '-3 months'), datetime('now'));

-- Team 11: Katelyn Helms (7 agents)
INSERT INTO agency_teams (Id, UId, Name, AgencySiteId, ManagerUserId, MemberCount, CreatedAt) VALUES
(11, 'team-helms-001', 'Team Helms', 2, 'charlotte-mgr-helms', 7, datetime('now'));

INSERT INTO users (Id, Name, Email, AgencySiteId, AgencyTeamId, Title, AgentStatus, AgentType, CommissionTier, AgencyStartDate, CreatedAt) VALUES
('charlotte-mgr-helms', 'Katelyn Helms', 'katelyn.helms@luminarylife.com', 2, 11, 2, 1, 2, 2, datetime('now', '-19 months'), datetime('now')),
('charlotte-agent-025', 'Beau Carson', 'beau.carson@luminarylife.com', 2, 11, 1, 1, 2, 1, datetime('now', '-10 months'), datetime('now')),
('charlotte-agent-026', 'Kyle Williford', 'kyle.williford@luminarylife.com', 2, 11, 1, 1, 2, 1, datetime('now', '-8 months'), datetime('now')),
('charlotte-agent-027', 'Don McCoy', 'don.mccoy@luminarylife.com', 2, 11, 1, 1, 2, 2, datetime('now', '-14 months'), datetime('now')),
('charlotte-agent-028', 'Bryon Griffin', 'bryon.griffin@luminarylife.com', 2, 11, 1, 1, 2, 1, datetime('now', '-6 months'), datetime('now')),
('charlotte-agent-029', 'Barion Jones', 'barion.jones@luminarylife.com', 2, 11, 1, 1, 2, 1, datetime('now', '-5 months'), datetime('now')),
('charlotte-agent-030', 'James Batton', 'james.batton@luminarylife.com', 2, 11, 1, 1, 2, 1, datetime('now', '-4 months'), datetime('now')),
('charlotte-agent-031', 'Ockeyana Williams', 'ockeyana.williams@luminarylife.com', 2, 11, 1, 1, 2, 1, datetime('now', '-7 months'), datetime('now'));

-- Team 12: Jacob Fuller (8 agents)
INSERT INTO agency_teams (Id, UId, Name, AgencySiteId, ManagerUserId, MemberCount, CreatedAt) VALUES
(12, 'team-fuller-001', 'Team Fuller', 2, 'charlotte-mgr-fuller', 8, datetime('now'));

INSERT INTO users (Id, Name, Email, AgencySiteId, AgencyTeamId, Title, AgentStatus, AgentType, CommissionTier, AgencyStartDate, CreatedAt) VALUES
('charlotte-mgr-fuller', 'Jacob Fuller', 'jacob.fuller@luminarylife.com', 2, 12, 2, 1, 2, 2, datetime('now', '-17 months'), datetime('now')),
('charlotte-agent-032', 'Jeff Rosenberg', 'jeff.rosenberg@luminarylife.com', 2, 12, 1, 1, 2, 1, datetime('now', '-9 months'), datetime('now')),
('charlotte-agent-033', 'Chris Williams', 'chris.williams@luminarylife.com', 2, 12, 1, 1, 2, 1, datetime('now', '-7 months'), datetime('now')),
('charlotte-agent-034', 'David Thompson', 'david.thompson@luminarylife.com', 2, 12, 1, 1, 2, 2, datetime('now', '-13 months'), datetime('now')),
('charlotte-agent-035', 'Gavin Stacks', 'gavin.stacks@luminarylife.com', 2, 12, 1, 1, 2, 1, datetime('now', '-6 months'), datetime('now')),
('charlotte-agent-036', 'Divearl Brown', 'divearl.brown@luminarylife.com', 2, 12, 1, 1, 2, 1, datetime('now', '-5 months'), datetime('now')),
('charlotte-agent-037', 'Shannon Little', 'shannon.little@luminarylife.com', 2, 12, 1, 1, 2, 1, datetime('now', '-4 months'), datetime('now')),
('charlotte-agent-038', 'Terrence Robinson', 'terrence.robinson@luminarylife.com', 2, 12, 1, 1, 2, 1, datetime('now', '-8 months'), datetime('now')),
('charlotte-agent-039', 'Alvin Fulmore', 'alvin.fulmore@luminarylife.com', 2, 12, 1, 1, 2, 1, datetime('now', '-3 months'), datetime('now'));

-- Team 13: Miguel Roman (7 agents)
INSERT INTO agency_teams (Id, UId, Name, AgencySiteId, ManagerUserId, MemberCount, CreatedAt) VALUES
(13, 'team-roman-001', 'Team Roman', 2, 'charlotte-mgr-roman', 7, datetime('now'));

INSERT INTO users (Id, Name, Email, AgencySiteId, AgencyTeamId, Title, AgentStatus, AgentType, CommissionTier, AgencyStartDate, CreatedAt) VALUES
('charlotte-mgr-roman', 'Miguel Roman', 'miguel.roman@luminarylife.com', 2, 13, 2, 1, 2, 2, datetime('now', '-16 months'), datetime('now')),
('charlotte-agent-040', 'Kevin Gilliard', 'kevin.gilliard@luminarylife.com', 2, 13, 1, 1, 2, 1, datetime('now', '-10 months'), datetime('now')),
('charlotte-agent-041', 'Tamara Hemmings', 'tamara.hemmings@luminarylife.com', 2, 13, 1, 1, 2, 1, datetime('now', '-8 months'), datetime('now')),
('charlotte-agent-042', 'Phillip Mcewen', 'phillip.mcewen@luminarylife.com', 2, 13, 1, 1, 2, 2, datetime('now', '-15 months'), datetime('now')),
('charlotte-agent-043', 'Tiffany Holliman', 'tiffany.holliman@luminarylife.com', 2, 13, 1, 1, 2, 1, datetime('now', '-6 months'), datetime('now')),
('charlotte-agent-044', 'Jaime Umanzor', 'jaime.umanzor@luminarylife.com', 2, 13, 1, 1, 2, 1, datetime('now', '-5 months'), datetime('now')),
('charlotte-agent-045', 'Traci Drummond', 'traci.drummond@luminarylife.com', 2, 13, 1, 1, 2, 1, datetime('now', '-4 months'), datetime('now')),
('charlotte-agent-046', 'Gregory Powell', 'gregory.powell@luminarylife.com', 2, 13, 1, 1, 2, 1, datetime('now', '-7 months'), datetime('now'));

-- Team 14: Montrell Morgan (7 agents)
INSERT INTO agency_teams (Id, UId, Name, AgencySiteId, ManagerUserId, MemberCount, CreatedAt) VALUES
(14, 'team-morgan-001', 'Team Morgan', 2, 'charlotte-mgr-morgan', 7, datetime('now'));

INSERT INTO users (Id, Name, Email, AgencySiteId, AgencyTeamId, Title, AgentStatus, AgentType, CommissionTier, AgencyStartDate, CreatedAt) VALUES
('charlotte-mgr-morgan', 'Montrell Morgan', 'montrell.morgan@luminarylife.com', 2, 14, 2, 1, 2, 2, datetime('now', '-18 months'), datetime('now')),
('charlotte-agent-047', 'Jimmie Royster IV', 'jimmie.royster@luminarylife.com', 2, 14, 1, 1, 2, 1, datetime('now', '-9 months'), datetime('now')),
('charlotte-agent-048', 'Michael Masuck', 'michael.masuck@luminarylife.com', 2, 14, 1, 1, 2, 1, datetime('now', '-7 months'), datetime('now')),
('charlotte-agent-049', 'Chad Gammons', 'chad.gammons@luminarylife.com', 2, 14, 1, 1, 2, 2, datetime('now', '-14 months'), datetime('now')),
('charlotte-agent-050', 'Robert Mulligan', 'robert.mulligan@luminarylife.com', 2, 14, 1, 1, 2, 1, datetime('now', '-6 months'), datetime('now')),
('charlotte-agent-051', 'Jabari McKnight', 'jabari.mcknight@luminarylife.com', 2, 14, 1, 1, 2, 1, datetime('now', '-5 months'), datetime('now')),
('charlotte-agent-052', 'Naimah German', 'naimah.german@luminarylife.com', 2, 14, 1, 1, 2, 1, datetime('now', '-4 months'), datetime('now')),
('charlotte-agent-053', 'Caleb McIntosh', 'caleb.mcintosh@luminarylife.com', 2, 14, 1, 1, 2, 1, datetime('now', '-8 months'), datetime('now'));

-- Team 15: Brent Lahti (6 agents)
INSERT INTO agency_teams (Id, UId, Name, AgencySiteId, ManagerUserId, MemberCount, CreatedAt) VALUES
(15, 'team-lahti-001', 'Team Lahti', 2, 'charlotte-mgr-lahti', 6, datetime('now'));

INSERT INTO users (Id, Name, Email, AgencySiteId, AgencyTeamId, Title, AgentStatus, AgentType, CommissionTier, AgencyStartDate, CreatedAt) VALUES
('charlotte-mgr-lahti', 'Brent Lahti', 'brent.lahti@luminarylife.com', 2, 15, 2, 1, 2, 2, datetime('now', '-15 months'), datetime('now')),
('charlotte-agent-054', 'Quanikko Fernandors', 'quanikko.fernandors@luminarylife.com', 2, 15, 1, 1, 2, 1, datetime('now', '-10 months'), datetime('now')),
('charlotte-agent-055', 'Derrick Horne', 'derrick.horne@luminarylife.com', 2, 15, 1, 1, 2, 1, datetime('now', '-8 months'), datetime('now')),
('charlotte-agent-056', 'Samantha Gentle', 'samantha.gentle@luminarylife.com', 2, 15, 1, 1, 2, 1, datetime('now', '-6 months'), datetime('now')),
('charlotte-agent-057', 'Celina Tuck', 'celina.tuck@luminarylife.com', 2, 15, 1, 1, 2, 1, datetime('now', '-5 months'), datetime('now')),
('charlotte-agent-058', 'Terrell Dillard', 'terrell.dillard@luminarylife.com', 2, 15, 1, 1, 2, 1, datetime('now', '-4 months'), datetime('now')),
('charlotte-agent-059', 'David Bedington', 'david.bedington@luminarylife.com', 2, 15, 1, 1, 2, 1, datetime('now', '-3 months'), datetime('now'));

-- ============================================
-- SUMMARY
-- ============================================
-- Austin: 1 Site Manager + 7 Team Managers + 38 Agents = 46 users
-- Charlotte: 1 Site Manager + 8 Team Managers + 51 Agents = 60 users
-- Total: 106 users across 15 teams and 2 sites
