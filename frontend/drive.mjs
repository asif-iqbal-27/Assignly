import { chromium } from "playwright";
import fs from "fs";

const outDir = "C:\\Users\\ASIF'S~1\\AppData\\Local\\Temp\\claude\\g---Net-again-Assignly\\b9c607e5-156c-490f-8f6e-64a16e3b81c5\\scratchpad\\screenshots\\v3";
fs.mkdirSync(outDir, { recursive: true });

const browser = await chromium.launch({ args: ["--no-sandbox"] });
const consoleErrors = [];
const pageErrors = [];

function trackErrors(page, tag) {
  page.on("console", (msg) => {
    if (msg.type() === "error") consoleErrors.push(`[${tag}] ${msg.text()}`);
  });
  page.on("pageerror", (err) => pageErrors.push(`[${tag}] ${err.message}`));
}

async function login(page, userName, password) {
  await page.goto("http://localhost:3000/login", { waitUntil: "networkidle" });
  await page.fill("#userName", userName);
  await page.fill("#password", password);
  await page.click('button[type="submit"]');
  await page.waitForURL(/\/(admin|teacher|student)/, { timeout: 10000 });
}

async function shot(page, name) {
  await page.screenshot({ path: `${outDir}/${name}.png`, fullPage: true });
  console.log("saved", name);
}

// ============ ADMIN ============
{
  const page = await browser.newPage();
  trackErrors(page, "admin");
  await login(page, "admin", "Admin123!");
  await page.waitForURL(/\/admin\/users/);
  await page.waitForSelector("table tbody tr");
  await shot(page, "01-admin-users");

  // Create a class through the UI
  await page.goto("http://localhost:3000/admin/classes", { waitUntil: "networkidle" });
  await page.waitForSelector("table tbody tr");
  await shot(page, "02-admin-classes");
  await page.fill("#name", "UI Smoke Class");
  await page.fill("#section", "Z");
  const [createClassResp] = await Promise.all([
    page.waitForResponse((r) => r.url().endsWith("/api/classes") && r.request().method() === "POST"),
    page.click('button:has-text("Create class")'),
  ]);
  const classJson = await createClassResp.json();
  console.log("created class:", classJson.id);
  await page.waitForSelector(".banner-success");
  await shot(page, "03-admin-classes-created");

  // Edit it
  await page.click(`tr:has-text("UI Smoke Class") button:has-text("Edit")`);
  await page.fill("#name", "UI Smoke Class Renamed");
  await Promise.all([
    page.waitForResponse((r) => r.url().includes("/api/classes/") && r.request().method() === "PUT"),
    page.click('button:has-text("Save changes")'),
  ]);
  await page.waitForSelector(".banner-success");
  await shot(page, "04-admin-classes-edited");

  // Create a subject in it
  await page.goto("http://localhost:3000/admin/subjects", { waitUntil: "networkidle" });
  await page.fill("#name", "UI Smoke Subject");
  await page.selectOption("#classId", { label: (await page.locator("#classId option", { hasText: "UI Smoke Class Renamed" }).textContent()) ?? "" });
  const [createSubjResp] = await Promise.all([
    page.waitForResponse((r) => r.url().endsWith("/api/subjects") && r.request().method() === "POST"),
    page.click('button:has-text("Create subject")'),
  ]);
  const subjJson = await createSubjResp.json();
  console.log("created subject:", subjJson.id);
  await page.waitForSelector(".banner-success");
  await shot(page, "05-admin-subjects-created");

  // class-subject-teachers: assign teacher1 to the new subject, view it, remove it
  await page.goto("http://localhost:3000/admin/class-subject-teachers", { waitUntil: "networkidle" });
  await page.selectOption("#newTeacherId", { label: "Rahim Sheikh (teacher1)" });
  await page.selectOption("#newSubjectId", { label: "UI Smoke Subject - UI Smoke Class Renamed" });
  await Promise.all([
    page.waitForResponse((r) => r.url().endsWith("/api/class-subject-teachers") && r.request().method() === "POST"),
    page.click('button:has-text("Assign")'),
  ]);
  await page.waitForSelector(".banner-success");
  await page.selectOption("#viewTeacherId", { label: "Rahim Sheikh (teacher1)" });
  await page.waitForSelector("table tbody tr");
  await shot(page, "06-admin-class-subject-teachers");

  // Clean up: remove the assignment, delete subject, delete class
  await page.click(`tr:has-text("UI Smoke Subject") button:has-text("Remove")`);
  await page.waitForSelector(".banner-success");

  await page.goto("http://localhost:3000/admin/subjects", { waitUntil: "networkidle" });
  await page.click(`tr:has-text("UI Smoke Subject") button:has-text("Delete")`);
  await page.waitForSelector(".banner-success");

  await page.goto("http://localhost:3000/admin/classes", { waitUntil: "networkidle" });
  await page.click(`tr:has-text("UI Smoke Class Renamed") button:has-text("Delete")`);
  await page.waitForSelector(".banner-success");
  await shot(page, "07-admin-cleanup-done");

  await page.close();
}

// ============ TEACHER ============
let smokeAssignmentTitle = "UI Smoke Assignment";
{
  const page = await browser.newPage();
  trackErrors(page, "teacher");
  await login(page, "teacher1", "Teacher123!");
  await page.waitForSelector("table tbody tr");
  await shot(page, "08-teacher-assignments");

  // Create assignment
  await page.click('a[href="/teacher/assignments/new"]');
  await page.waitForURL(/\/teacher\/assignments\/new/);
  await page.fill("#title", smokeAssignmentTitle);
  await page.fill("#description", "Created by the UI verification script.");
  await page.selectOption("#subjectId", { label: "Mathematics - Class 10" });
  await page.fill("#deadline", "2027-06-01T10:00");
  await page.fill("#maxMarks", "20");
  await shot(page, "09-teacher-new-assignment-form");
  await Promise.all([
    page.waitForURL(/\/teacher$/),
    page.click('button:has-text("Create assignment")'),
  ]);
  await page.waitForSelector("table tbody tr");
  await shot(page, "10-teacher-assignments-after-create");

  // Publish it
  await page.click(`tr:has-text("${smokeAssignmentTitle}") button:has-text("Publish")`);
  await page.waitForSelector(".banner-success");
  await shot(page, "11-teacher-published");

  // Edit it
  await page.click(`tr:has-text("${smokeAssignmentTitle}") a:has-text("Edit")`);
  await page.waitForURL(/\/teacher\/assignments\/.+\/edit/);
  await page.waitForSelector("#title");
  await page.fill("#maxMarks", "25");
  await Promise.all([page.waitForURL(/\/teacher$/), page.click('button:has-text("Save changes")')]);
  await shot(page, "12-teacher-after-edit");

  // View submissions on an existing assignment with data (Newton's Laws)
  await page.click(`tr:has-text("Newton's Laws") a:has-text("Submissions")`);
  await page.waitForURL(/\/submissions$/);
  await page.waitForSelector("table tbody tr");
  await shot(page, "13-teacher-submissions-grading");

  await page.close();
}

// ============ STUDENT ============
{
  const page = await browser.newPage();
  trackErrors(page, "student");
  await login(page, "student1", "Student123!");
  await page.waitForSelector("table tbody tr");
  await shot(page, "14-student-assignments");

  // Open the smoke assignment and submit
  const [detailResp] = await Promise.all([
    page.waitForResponse((r) => r.url().endsWith("/api/submissions/mine")),
    page.click(`tr:has-text("${smokeAssignmentTitle}") button:has-text("View")`),
  ]);
  await detailResp.finished();
  await page.waitForTimeout(300);
  await shot(page, "15-student-assignment-detail-before-submit");

  await page.fill("#content", "My answer for the UI smoke test assignment.");
  await Promise.all([
    page.waitForResponse((r) => /\/api\/assignments\/.+\/submissions/.test(r.url())),
    page.click('button:has-text("Submit")'),
  ]);
  await page.waitForSelector(".banner-success");
  await shot(page, "16-student-after-submit");

  // My submissions
  await page.goto("http://localhost:3000/student/submissions", { waitUntil: "networkidle" });
  await page.waitForSelector("table tbody tr");
  await shot(page, "17-student-my-submissions");

  await page.close();
}

// ============ Narrow viewport ============
{
  const page = await browser.newPage({ viewport: { width: 390, height: 844 } });
  trackErrors(page, "narrow");
  await login(page, "admin", "Admin123!");
  await page.waitForSelector("table tbody tr");
  await shot(page, "18-narrow-admin-users");
  await page.close();
}

await browser.close();

console.log("\n--- console errors ---");
console.log(consoleErrors.length === 0 ? "none" : consoleErrors.join("\n"));
console.log("\n--- page errors (uncaught exceptions) ---");
console.log(pageErrors.length === 0 ? "none" : pageErrors.join("\n"));
