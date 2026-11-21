# Quickstart & Validation Guide: Platform Enhancements

**Feature Branch**: `004-platform-enhancements`  
**Date**: 2026-11-21  

This guide provides end-to-end validation scenarios and test steps for the 5 implemented features.

---

## 1. Prerequisites

1. SQL Server running with `MovieManagementSystem` database.
2. Web API running at `https://localhost:7154` (or configured port).
3. Blazor Server running at `https://localhost:7216`.

---

## 2. Validation Scenario 1: Video Resume & Timestamp Remembering

### Steps:
1. Log in to the application at `/login`.
2. Navigate to `/drama/1` (e.g. Alchemy of Souls) and click **Play Episode 1**.
3. Allow the video to play to `00:45` or fast-forward to `01:30`.
4. Navigate away by clicking **Home** in the navigation bar.
5. In the **Continue Watching** carousel on the home page, verify that Episode 1 displays the saved progress percentage and timestamp.
6. Click the title to open `DramaDetail.razor`.
7. Click **Resume** or **Play Episode 1**.

### Expected Outcome:
- Video player initializes and automatically seeks directly to `01:30`.
- A transient badge or toast appears: *"Resumed from 01:30"*.

---

## 3. Validation Scenario 2: Myanmar Subtitle Track (.vtt)

### Steps:
1. Open an episode that has an associated `.vtt` file (e.g. `alchemy_ep1_my.vtt` in `/subtitles/`).
2. Verify the player has captions/subtitles enabled by default or available in the track selector.
3. Click the subtitle icon or inspect `<video>` tracks in developer tools.

### Expected Outcome:
- `<track kind="subtitles" srclang="my" label="Myanmar (မြန်မာ)">` is attached.
- Myanmar Unicode subtitle text renders accurately on screen during playback without broken text shaping.

---

## 4. Validation Scenario 3: Advanced Filtering & Pagination (Category View)

### Steps:
1. Navigate to `/dramas/category/all` or `/dramas/category/korean`.
2. In the filter bar, select:
   - **Year**: `2024`
   - **Status**: `Completed`
   - **Rating**: `8.0+`
3. Enter a title search in the search box.
4. Verify the filtered list displays only items meeting all selected facets.
5. If the result count exceeds the page size (12 items), verify page numbers `[1]`, `[2]`, `[Next]` appear and navigating pages scrolls smoothly to top.

### Expected Outcome:
- Instant client-side filtering without page reloads.
- Page indicator displays active page (e.g., `Page 1 of 3`).
- Clear filter button resets all facets back to `All`.

---

## 5. Validation Scenario 4: User Profile & Self-Service Password Change

### Steps:
1. Log in as an ordinary user or admin.
2. Click the user avatar in the top navbar and select **Profile** (or navigate to `/profile`).
3. Select a new Dreamy Pastel avatar preset and update the Display Name; click **Save Changes**.
4. In the Password section:
   - Enter incorrect current password -> Click **Change Password** -> Verify error *"Current password is incorrect"*.
   - Enter correct current password, enter new password `NewPass@2026`, confirm new password -> Click **Change Password** -> Verify success message.
5. Log out and log back in using `NewPass@2026`.

### Expected Outcome:
- Profile updates are saved immediately and navbar displays new avatar.
- Login succeeds with new password.

---

## 6. Validation Scenario 5: Activity Log / Audit Trail

### Steps:
1. Log in as an Administrator.
2. Go to `/admin/movies` and delete or create a test movie/content.
3. Navigate to `/admin/audit-logs`.
4. Inspect the table.

### Expected Outcome:
- An audit entry is visible displaying:
  - Admin Username
  - Action (e.g., `DELETE_MOVIE`)
  - Target Entity (`Movie`)
  - Target ID
  - UTC Timestamp
- Filter by Action (`DELETE_MOVIE`) and verify only matching events are returned.
- Non-admin user attempting to access `/admin/audit-logs` is denied access.
