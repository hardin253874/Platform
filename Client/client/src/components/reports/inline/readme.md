# Inline Editing in Reports

This feature is being added on with attempted minimal impact to existing 
code and functionality so we can have it "feature switched" off until ready. 
Just saying as it might explain some of the implementation choices you see 
in the code.

## Overview

Some notes here to help show how the components involved work together.

-   The report directive shows our data grid directive that uses a ng data grid
    to render the report data.
-   We have the concept of an inline editing session held by the report directive
    when it enters inline editing mode.
-   The session is a list of resource editing states, each being the resource entity,
    the form entity used, and other info about the state of editing for that resource.
-   The data grid has a html template to use for each cell. This template includes
    a request to the report to check if editing and if so then to get an alternate
    template to use. This is an area to improve on in the implementation.
-   If a cell is being inline edited then we use a template that is a
    report inline edit control and this works together with the report (and thus
    the inline session service) to access the form and resource data to edit with.
-   The report's inline edit control determines which child form control to use
    for it report column and shows an inline edit form component on that control
    (and form and resource).

Some notes on behaviour (maybe to go into a functional spec/stories list)

-   When saving
    -   each changed resource is validated and saved
        -   if either fail then the resource is marked as failed
        -   if a validation issue then the control rendering shows an indicator
    -   if any resource has failed then an error alert is shown saying how many
        and we stay in edit mode and we don't refresh the report
        -   not refreshing the report means we don't see changes to the records
            that did get saved
-   Row status in edit mode is
    -   if the current row then show editable control like a form's edit mode
    -   if the row is not current but has changes then it is visually different
    -   if the row last save attempt failed then it is visually different
        -   however if a change is made to the row then the error state is cleared
            and it returns to the "changed" state
-   Editing a row of a report on a screen that is linked to a form on the screen
    should cause the form to refresh after the inline edit is saved
-   No inline editing on summary reports

## Known Issues and Behaviour Limitations

-   if one or more records in a session fails to save then we stay in edit mode
    and we don't refresh the report, therefore changes to records that did save
    won't be seen until you leave edit mode and refresh the report
-   if a calculated field exists on the report then it will not be updated until
    the editing session is saved. That is, if the calc is dependent upon another
    field that is edited then the calc will be incorrect until the editing session
    is done. Maybe we should hide calculated fields on edited rows?

## Limitations for this phase

-   not doing date/time/datetime
-   cannot nav away when in inline mode (+new, links for lookups)
-   not supporting mobile

## To Do

- [ ] testing on standalone reports
- [ ] testing reports on forms
- [ ] testing reports on screens
- [ ] test with large reports when have 100s of rows
- [ ] Make the error handling really good. We are going to see errors due to forms
    not being suitable for editing in a given report, or data missing mandatory
    fields that aren't in the report
- [ ] Improve perceived performance of a row going into edit mode
- [ ] Review performance impact of inline editing on a grid when not editing
- [ ] Write some jasmine unit test cases
- [ ] Implement RT drivers and basic RT tests ready for QA
- [ ] Support on screens and handling navigating away and back to the screen
- [ ] Ensure good keyboard access, tab to move across, enter to move down
- [ ] Do optimistic update of the underlying grid data when saving

#### Issues

- [ ] Multi line text doesn't render well in view mode
- [ ] Multi choice doesn't support tab focus

