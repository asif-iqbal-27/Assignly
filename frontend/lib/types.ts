// These mirror the backend DTOs in Assignly.Application/Dtos exactly.
// Enums come over the wire as strings (JsonStringEnumConverter), not numbers.

export type Role = "Admin" | "Teacher" | "Student";

export type AssignmentStatus = "Draft" | "Published";

export type SubmissionStatus = "Submitted" | "Late" | "UnderReview" | "Graded";

export interface AuthResponse {
  token: string;
  userName: string;
  role: Role;
}

export interface UserDto {
  id: string;
  userName: string;
  email: string;
  fullName: string;
  role: Role;
  classId: string | null;
  className: string | null;
  isActive: boolean;
}

export interface ClassDto {
  id: string;
  name: string;
  section: string | null;
  description: string | null;
}

export interface SubjectDto {
  id: string;
  name: string;
  classId: string;
  className: string;
}

export interface ClassSubjectTeacherDto {
  id: string;
  teacherId: string;
  teacherName: string;
  subjectId: string;
  subjectName: string;
}

export interface AssignmentDto {
  id: string;
  title: string;
  description: string;
  subjectId: string;
  subjectName: string;
  classId: string;
  className: string;
  createdByTeacherId: string;
  createdByTeacherName: string;
  deadline: string;
  maxMarks: number;
  status: AssignmentStatus;
  allowLateSubmission: boolean;
  allowResubmission: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SubmissionDto {
  id: string;
  assignmentId: string;
  assignmentTitle: string;
  studentId: string;
  studentName: string;
  content: string | null;
  fileUrl: string | null;
  submittedAt: string;
  isLate: boolean;
  attemptNumber: number;
  status: SubmissionStatus;
  marks: number | null;
  feedback: string | null;
  gradedByTeacherId: string | null;
  gradedByTeacherName: string | null;
  gradedAt: string | null;
}
