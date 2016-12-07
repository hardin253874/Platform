# Software Platform HTML5 Client

This document is the front page of the generated documentation for the
Software Platform HTML5 Client project.


## Things to be cleaned up

The following are some notes on what we can tidy up in our
codebase. This is a living list and dump of observations.

### Entity model and services

*   tidy up the handling of id, eid, entityref, alias, namespace or 
    not, etc.

*   find client code using entity internals, things like entity._id._id
    and have them do what they need via a proper public interface,
    creating the interface if needed
    
*   move client code to use the object properties for accessing
    fields and relationships rather than the "legacy" getField 
    and getRelationship methods
    
    
   
    

