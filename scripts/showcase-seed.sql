-- ============================================================================
--  WebIde showcase seed data
--  Additive + idempotent (ON CONFLICT DO NOTHING). Uses explicit IDs in the
--  100+/1000+ range to avoid colliding with any app-seeded rows, then bumps
--  the identity sequences so future app inserts don't clash.
--
--  Enum integer values (from the model):
--    DifficultyLevel : Easy=0  Medium=1  Hard=2
--    UserRole        : Admin=0 Instructor=1 Student=2
--    SubmissionStatus: Accepted=2 WrongAnswer=3 TLE=4 MLE=5 CompileError=6 RuntimeError=7
--    Verdict         : Accepted=1 WrongAnswer=2 TLE=3 MLE=4 RuntimeError=5 CompileError=6
-- ============================================================================
BEGIN;

-- ── Domain users ────────────────────────────────────────────────────────────
INSERT INTO "DomainUsers" ("Id","Username","Email","DisplayName","Role","RegisteredAt","AvatarUrl") VALUES
 (101,'alice','alice@zetesis.cc','Alice Kovač',        2, now() - interval '40 days', 'https://i.pravatar.cc/150?img=1'),
 (102,'bob','bob@zetesis.cc','Bob Novak',              2, now() - interval '37 days', 'https://i.pravatar.cc/150?img=12'),
 (103,'carol','carol@zetesis.cc','Carol Horvat',       1, now() - interval '55 days', 'https://i.pravatar.cc/150?img=5'),
 (104,'dave','dave@zetesis.cc','Dave Marić',           2, now() - interval '20 days', 'https://i.pravatar.cc/150?img=15'),
 (105,'erin','erin@zetesis.cc','Erin Babić',           2, now() - interval '12 days', 'https://i.pravatar.cc/150?img=9'),
 (106,'zoran','zoran@zetesis.cc','Prof. Zoran Perić',  1, now() - interval '70 days', 'https://i.pravatar.cc/150?img=33')
ON CONFLICT ("Id") DO NOTHING;

-- ── Tags ────────────────────────────────────────────────────────────────────
INSERT INTO "Tags" ("Id","Name") VALUES
 (101,'Math'),(102,'Strings'),(103,'Arrays'),(104,'Implementation'),
 (105,'Number Theory'),(106,'Dynamic Programming')
ON CONFLICT ("Id") DO NOTHING;

-- ── Problems ────────────────────────────────────────────────────────────────
INSERT INTO "Problems" ("Id","Title","Description","Difficulty","TimeLimitMs","MemoryLimitKb","FloatTolerance","CreatedAt","AuthorUsername") VALUES
 (101,'Sum of Two Numbers', $$Read two space-separated integers `a` and `b` on one line and print their sum.

**Input:** `a b`
**Output:** `a + b`

**Example:** input `3 5` → output `8`$$, 0, 1000, 262144, NULL, now() - interval '60 days','zoran'),
 (102,'Sum to N', $$Given an integer `n`, print the sum 1 + 2 + ... + n.

**Input:** a single integer `n` (1 ≤ n ≤ 10^6)
**Output:** the sum.

**Example:** input `5` → output `15`$$, 0, 1000, 262144, NULL, now() - interval '58 days','zoran'),
 (103,'Reverse a String', $$Read a single line and print it reversed.

**Example:** input `hello` → output `olleh`$$, 0, 1000, 262144, NULL, now() - interval '54 days','carol'),
 (104,'Factorial', $$Read an integer `n` (0 ≤ n ≤ 12) and print `n!`.

**Example:** input `5` → output `120`$$, 0, 1000, 262144, NULL, now() - interval '50 days','carol'),
 (105,'FizzBuzz', $$Print the numbers from 1 to `n`, one per line, but for multiples of 3 print `Fizz`, for multiples of 5 print `Buzz`, and for multiples of both print `FizzBuzz`.

**Example:** input `5` → output `1 / 2 / Fizz / 4 / Buzz` (each on its own line)$$, 1, 1000, 262144, NULL, now() - interval '45 days','zoran'),
 (106,'Maximum in Array', $$The first line contains `n`, the second line contains `n` space-separated integers. Print the largest.

**Example:** input `5` then `3 1 4 1 5` → output `5`$$, 0, 1000, 262144, NULL, now() - interval '40 days','carol'),
 (107,'Count Vowels', $$Read a line of text and print how many vowels (a, e, i, o, u) it contains.

**Example:** input `hello world` → output `3`$$, 0, 1000, 262144, NULL, now() - interval '33 days','carol'),
 (108,'Nth Fibonacci', $$Print the n-th Fibonacci number, where F(0)=0, F(1)=1.

**Example:** input `10` → output `55`$$, 1, 1000, 262144, NULL, now() - interval '28 days','zoran'),
 (109,'Greatest Common Divisor', $$Read two integers and print their greatest common divisor.

**Example:** input `12 18` → output `6`$$, 1, 1000, 262144, NULL, now() - interval '21 days','zoran'),
 (110,'Circle Area', $$Read a real number `r` (the radius) and print the area of the circle, π·r². Answers within 1e-4 are accepted.

**Example:** input `1` → output `3.14159265`$$, 1, 1000, 262144, 0.0001, now() - interval '14 days','carol')
ON CONFLICT ("Id") DO NOTHING;

-- ── Test cases (3 per problem; first is the public sample; points sum to 100) ─
INSERT INTO "TestCases" ("Id","InputArgs","ExpectedOutput","IsSample","OrderIndex","Points","ProblemId") VALUES
 -- 101 Sum of Two
 (1001,'3 5','8',     true ,0,40,101),(1002,'10 20','30',  false,1,30,101),(1003,'-7 7','0',  false,2,30,101),
 -- 102 Sum to N
 (1004,'5','15',      true ,0,40,102),(1005,'1','1',       false,1,30,102),(1006,'100','5050',false,2,30,102),
 -- 103 Reverse a String
 (1007,'hello','olleh',true,0,40,103),(1008,'abc','cba',   false,1,30,103),(1009,'racecar','racecar',false,2,30,103),
 -- 104 Factorial
 (1010,'5','120',    true ,0,40,104),(1011,'0','1',        false,1,30,104),(1012,'6','720', false,2,30,104),
 -- 105 FizzBuzz
 (1013,'5',E'1\n2\nFizz\n4\nBuzz',true,0,40,105),
 (1014,'3',E'1\n2\nFizz',         false,1,30,105),
 (1015,'15',E'1\n2\nFizz\n4\nBuzz\nFizz\n7\n8\nFizz\nBuzz\n11\nFizz\n13\n14\nFizzBuzz',false,2,30,105),
 -- 106 Maximum in Array
 (1016,E'5\n3 1 4 1 5','5', true ,0,40,106),(1017,E'3\n-1 -2 -3','-1',false,1,30,106),(1018,E'1\n42','42',false,2,30,106),
 -- 107 Count Vowels
 (1019,'hello world','3',true,0,40,107),(1020,'xyz','0',  false,1,30,107),(1021,'aeiou','5',false,2,30,107),
 -- 108 Nth Fibonacci
 (1022,'10','55',    true ,0,40,108),(1023,'1','1',       false,1,30,108),(1024,'0','0',   false,2,30,108),
 -- 109 GCD
 (1025,'12 18','6',  true ,0,40,109),(1026,'7 13','1',    false,1,30,109),(1027,'100 80','20',false,2,30,109),
 -- 110 Circle Area
 (1028,'1','3.14159265',true,0,40,110),(1029,'2','12.5663706',false,1,30,110),(1030,'0','0',false,2,30,110)
ON CONFLICT ("Id") DO NOTHING;

-- ── Problem ↔ Tag links ─────────────────────────────────────────────────────
INSERT INTO "ProblemTags" ("ProblemsId","TagsId") VALUES
 (101,101),(101,104),
 (102,101),
 (103,102),(103,104),
 (104,101),(104,105),
 (105,104),
 (106,103),(106,104),
 (107,102),
 (108,101),(108,106),
 (109,101),(109,105),
 (110,101)
ON CONFLICT DO NOTHING;

-- ── Organizations ───────────────────────────────────────────────────────────
INSERT INTO "Organizations" ("Id","Name","Description") VALUES
 (101,'Zetesis Academy','Internal training organization for competitive programming.'),
 (102,'Open Practice','Public practice space — open to everyone.')
ON CONFLICT ("Id") DO NOTHING;

-- ── Organization members ────────────────────────────────────────────────────
INSERT INTO "OrganizationMembers" ("MembersId","OrganizationsId") VALUES
 (101,101),(102,101),(103,101),(106,101),
 (104,102),(105,102),(101,102)
ON CONFLICT DO NOTHING;

-- ── Problem sets ────────────────────────────────────────────────────────────
INSERT INTO "ProblemSets" ("Id","Title","Description","CreatedAt","IsPublic","OrderIndex","OrganizationId") VALUES
 (101,'Beginner Track','Warm-up problems for newcomers.',           now() - interval '50 days', true, 0, 101),
 (102,'Weekly Contest #1','First weekly contest set.',              now() - interval '20 days', true, 1, 101),
 (103,'Math & Number Theory','Selected math problems.',             now() - interval '15 days', true, 0, 102)
ON CONFLICT ("Id") DO NOTHING;

-- ── Problem set ↔ Problem links ─────────────────────────────────────────────
INSERT INTO "ProblemSetProblems" ("ProblemSetId","ProblemsId") VALUES
 (101,101),(101,102),(101,103),(101,104),(101,107),
 (102,105),(102,106),(102,108),
 (103,102),(103,104),(103,109),(103,110)
ON CONFLICT DO NOTHING;

-- ── Submissions (varied outcomes for a lively leaderboard / activity feed) ───
--  Status: 2=Accepted 3=WrongAnswer 4=TLE
INSERT INTO "Submissions" ("Id","SourceCode","Language","Status","SubmittedAt","Score","WallTimeMs","PeakMemoryKb","UserId","ProblemId") VALUES
 (1001,$$a,b=map(int,input().split())
print(a+b)$$,'python',2, now() - interval '39 days', 100, 18, 8800,101,101),
 (1002,$$print(sum(range(int(input())+1)))$$,'python',2, now() - interval '38 days', 100, 15, 8600,102,102),
 (1003,$$print(input()[::-1])$$,'python',2, now() - interval '34 days', 100, 14, 8700,101,103),
 (1004,$$#include<iostream>
int main(){int n;std::cin>>n;long f=1;for(int i=2;i<=n;i++)f*=i;std::cout<<f;}$$,'cpp',2, now() - interval '33 days', 100, 6, 4200,104,104),
 (1005,$$for i in range(1,int(input())+1):
    print('FizzBuzz' if i%15==0 else 'Fizz' if i%3==0 else 'Buzz' if i%5==0 else i)$$,'python',2, now() - interval '30 days', 100, 22, 9100,103,105),
 (1006,$$input();print(max(map(int,input().split())))$$,'python',2, now() - interval '29 days', 100, 17, 8900,102,106),
 (1007,$$print(sum(c in 'aeiou' for c in input()))$$,'python',2, now() - interval '26 days', 100, 16, 8800,105,107),
 (1008,$$def f(n):
    a,b=0,1
    for _ in range(n):a,b=b,a+b
    return a
print(f(int(input())))$$,'python',2, now() - interval '24 days', 100, 19, 9000,101,108),
 (1009,$$import math
print(math.gcd(*map(int,input().split())))$$,'python',2, now() - interval '20 days', 100, 15, 8700,104,109),
 (1010,$$import math
print(f"{math.pi*float(input())**2:.8f}")$$,'python',2, now() - interval '13 days', 100, 18, 9200,105,110),
 -- some non-perfect results for realism
 (1011,$$a,b=input().split()
print(a+b)$$,'python',3, now() - interval '39 days', 40, 16, 8600,102,101),   -- string concat bug -> WrongAnswer
 (1012,$$n=int(input())
print(n*(n+1)//2 + 1)$$,'python',3, now() - interval '37 days', 30, 15, 8700,104,102), -- off by one
 (1013,$$while True:
    pass$$,'python',4, now() - interval '30 days', 0, 1000, 9000,105,105),                 -- TLE
 (1014,$$print(input()[::-1])$$,'python',2, now() - interval '10 days', 100, 13, 8600,105,103),
 (1015,$$a,b=map(int,input().split());print(a+b)$$,'python',2, now() - interval '9 days', 100, 14, 8500,104,101),
 (1016,$$input();print(max(map(int,input().split())))$$,'python',2, now() - interval '8 days', 100, 16, 8800,101,106),
 (1017,$$import math;print(math.gcd(*map(int,input().split())))$$,'python',2, now() - interval '5 days', 100, 15, 8700,102,109),
 (1018,$$#include<bits/stdc++.h>
int main(){long n;std::cin>>n;std::cout<<n*(n+1)/2;}$$,'cpp',2, now() - interval '2 days', 100, 5, 4100,101,102)
ON CONFLICT ("Id") DO NOTHING;

-- ── One ExecutionResult per submission (so the submission detail page renders) ─
INSERT INTO "ExecutionResults"
 ("Id","SubmissionId","TestCaseId","Stdout","Stderr","ExitCode","WallTimeMs","PeakMemoryKb","Verdict","TimedOut","MemoryExceeded")
SELECT 5000 + row_number() OVER (ORDER BY s."Id"),
       s."Id",
       (SELECT min(tc."Id") FROM "TestCases" tc WHERE tc."ProblemId" = s."ProblemId"),
       '', '',
       CASE WHEN s."Status"=6 THEN 2 ELSE 0 END,
       s."WallTimeMs", s."PeakMemoryKb",
       CASE s."Status" WHEN 2 THEN 1 WHEN 3 THEN 2 WHEN 4 THEN 3 WHEN 5 THEN 4 WHEN 7 THEN 5 WHEN 6 THEN 6 ELSE 1 END,
       (s."Status"=4), (s."Status"=5)
FROM "Submissions" s
WHERE s."Id" BETWEEN 1001 AND 1099
  AND NOT EXISTS (SELECT 1 FROM "ExecutionResults" er WHERE er."SubmissionId"=s."Id");

UPDATE "Submissions" s
SET "ExecutionResultId" = er."Id"
FROM "ExecutionResults" er
WHERE er."SubmissionId" = s."Id" AND s."Id" BETWEEN 1001 AND 1099 AND s."ExecutionResultId" IS NULL;

-- ── Bump identity sequences past the explicit IDs we inserted ────────────────
SELECT setval(pg_get_serial_sequence('"DomainUsers"','Id'),       GREATEST((SELECT max("Id") FROM "DomainUsers"),1));
SELECT setval(pg_get_serial_sequence('"Tags"','Id'),              GREATEST((SELECT max("Id") FROM "Tags"),1));
SELECT setval(pg_get_serial_sequence('"Problems"','Id'),          GREATEST((SELECT max("Id") FROM "Problems"),1));
SELECT setval(pg_get_serial_sequence('"TestCases"','Id'),         GREATEST((SELECT max("Id") FROM "TestCases"),1));
SELECT setval(pg_get_serial_sequence('"Organizations"','Id'),     GREATEST((SELECT max("Id") FROM "Organizations"),1));
SELECT setval(pg_get_serial_sequence('"ProblemSets"','Id'),       GREATEST((SELECT max("Id") FROM "ProblemSets"),1));
SELECT setval(pg_get_serial_sequence('"Submissions"','Id'),       GREATEST((SELECT max("Id") FROM "Submissions"),1));
SELECT setval(pg_get_serial_sequence('"ExecutionResults"','Id'),  GREATEST((SELECT max("Id") FROM "ExecutionResults"),1));

COMMIT;

-- ── Summary ─────────────────────────────────────────────────────────────────
SELECT 'Problems' t, count(*) n FROM "Problems" UNION ALL
SELECT 'TestCases', count(*) FROM "TestCases" UNION ALL
SELECT 'DomainUsers', count(*) FROM "DomainUsers" UNION ALL
SELECT 'Tags', count(*) FROM "Tags" UNION ALL
SELECT 'Organizations', count(*) FROM "Organizations" UNION ALL
SELECT 'ProblemSets', count(*) FROM "ProblemSets" UNION ALL
SELECT 'Submissions', count(*) FROM "Submissions" UNION ALL
SELECT 'ExecutionResults', count(*) FROM "ExecutionResults";
