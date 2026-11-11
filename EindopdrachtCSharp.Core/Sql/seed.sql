-- Voorbeeldgegevens; opnieuw uitvoeren voegt bestaande namen niet dubbel toe.
INSERT INTO Personen (Naam, Leeftijd) SELECT 'Sanne de Vries', 25 WHERE NOT EXISTS (SELECT 1 FROM Personen WHERE Naam = 'Sanne de Vries');
INSERT INTO Personen (Naam, Leeftijd) SELECT 'Mohammed Bakker', 31 WHERE NOT EXISTS (SELECT 1 FROM Personen WHERE Naam = 'Mohammed Bakker');
INSERT INTO Personen (Naam, Leeftijd) SELECT 'Emma Jansen', 22 WHERE NOT EXISTS (SELECT 1 FROM Personen WHERE Naam = 'Emma Jansen');
INSERT INTO Personen (Naam, Leeftijd) SELECT 'Liam Visser', 36 WHERE NOT EXISTS (SELECT 1 FROM Personen WHERE Naam = 'Liam Visser');
INSERT INTO Personen (Naam, Leeftijd) SELECT 'Noa Smit', 28 WHERE NOT EXISTS (SELECT 1 FROM Personen WHERE Naam = 'Noa Smit');
INSERT INTO Personen (Naam, Leeftijd) SELECT 'Daan de Boer', 40 WHERE NOT EXISTS (SELECT 1 FROM Personen WHERE Naam = 'Daan de Boer');
INSERT INTO Personen (Naam, Leeftijd) SELECT 'Sara van Dijk', 24 WHERE NOT EXISTS (SELECT 1 FROM Personen WHERE Naam = 'Sara van Dijk');
INSERT INTO Personen (Naam, Leeftijd) SELECT 'Milan Meijer', 34 WHERE NOT EXISTS (SELECT 1 FROM Personen WHERE Naam = 'Milan Meijer');
INSERT INTO Personen (Naam, Leeftijd) SELECT 'Lisa Bos', 29 WHERE NOT EXISTS (SELECT 1 FROM Personen WHERE Naam = 'Lisa Bos');
INSERT INTO Personen (Naam, Leeftijd) SELECT 'Jayden Mulder', 27 WHERE NOT EXISTS (SELECT 1 FROM Personen WHERE Naam = 'Jayden Mulder');

INSERT INTO Opleidingen (Naam) SELECT 'Informatica' WHERE NOT EXISTS (SELECT 1 FROM Opleidingen WHERE Naam = 'Informatica');
INSERT INTO Opleidingen (Naam) SELECT 'Elektrotechniek' WHERE NOT EXISTS (SELECT 1 FROM Opleidingen WHERE Naam = 'Elektrotechniek');
INSERT INTO Opleidingen (Naam) SELECT 'Bedrijfskunde' WHERE NOT EXISTS (SELECT 1 FROM Opleidingen WHERE Naam = 'Bedrijfskunde');
INSERT INTO Opleidingen (Naam) SELECT 'Verpleegkunde' WHERE NOT EXISTS (SELECT 1 FROM Opleidingen WHERE Naam = 'Verpleegkunde');
INSERT INTO Opleidingen (Naam) SELECT 'Werktuigbouwkunde' WHERE NOT EXISTS (SELECT 1 FROM Opleidingen WHERE Naam = 'Werktuigbouwkunde');
INSERT INTO Opleidingen (Naam) SELECT 'Communicatie' WHERE NOT EXISTS (SELECT 1 FROM Opleidingen WHERE Naam = 'Communicatie');
INSERT INTO Opleidingen (Naam) SELECT 'Rechten' WHERE NOT EXISTS (SELECT 1 FROM Opleidingen WHERE Naam = 'Rechten');
INSERT INTO Opleidingen (Naam) SELECT 'Marketing' WHERE NOT EXISTS (SELECT 1 FROM Opleidingen WHERE Naam = 'Marketing');
INSERT INTO Opleidingen (Naam) SELECT 'Onderwijs' WHERE NOT EXISTS (SELECT 1 FROM Opleidingen WHERE Naam = 'Onderwijs');
INSERT INTO Opleidingen (Naam) SELECT 'Finance' WHERE NOT EXISTS (SELECT 1 FROM Opleidingen WHERE Naam = 'Finance');

INSERT INTO Beroepen (Naam) SELECT 'Softwareontwikkelaar' WHERE NOT EXISTS (SELECT 1 FROM Beroepen WHERE Naam = 'Softwareontwikkelaar');
INSERT INTO Beroepen (Naam) SELECT 'Elektrotechnisch ingenieur' WHERE NOT EXISTS (SELECT 1 FROM Beroepen WHERE Naam = 'Elektrotechnisch ingenieur');
INSERT INTO Beroepen (Naam) SELECT 'Projectmanager' WHERE NOT EXISTS (SELECT 1 FROM Beroepen WHERE Naam = 'Projectmanager');
INSERT INTO Beroepen (Naam) SELECT 'Verpleegkundige' WHERE NOT EXISTS (SELECT 1 FROM Beroepen WHERE Naam = 'Verpleegkundige');
INSERT INTO Beroepen (Naam) SELECT 'Werktuigbouwkundig ingenieur' WHERE NOT EXISTS (SELECT 1 FROM Beroepen WHERE Naam = 'Werktuigbouwkundig ingenieur');
INSERT INTO Beroepen (Naam) SELECT 'Communicatieadviseur' WHERE NOT EXISTS (SELECT 1 FROM Beroepen WHERE Naam = 'Communicatieadviseur');
INSERT INTO Beroepen (Naam) SELECT 'Jurist' WHERE NOT EXISTS (SELECT 1 FROM Beroepen WHERE Naam = 'Jurist');
INSERT INTO Beroepen (Naam) SELECT 'Marketeer' WHERE NOT EXISTS (SELECT 1 FROM Beroepen WHERE Naam = 'Marketeer');
INSERT INTO Beroepen (Naam) SELECT 'Docent' WHERE NOT EXISTS (SELECT 1 FROM Beroepen WHERE Naam = 'Docent');
INSERT INTO Beroepen (Naam) SELECT 'Financieel analist' WHERE NOT EXISTS (SELECT 1 FROM Beroepen WHERE Naam = 'Financieel analist');
