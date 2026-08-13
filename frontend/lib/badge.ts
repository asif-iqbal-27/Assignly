// Maps a status string straight from the API to the CSS class that colors
// its pill. Purely presentational - it never affects what data is fetched.
export function badgeClass(status: string): string {
  switch (status) {
    case "Published":
    case "Graded":
    case "Active":
      return "badge badge-green";
    case "Draft":
    case "Inactive":
      return "badge badge-gray";
    case "Submitted":
      return "badge badge-blue";
    case "Late":
      return "badge badge-amber";
    case "UnderReview":
      return "badge badge-purple";
    default:
      return "badge badge-gray";
  }
}
