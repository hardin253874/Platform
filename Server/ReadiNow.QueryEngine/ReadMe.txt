ReadiNow.QueryEngine
~~~~~~~~~~~~~~~~~~~~

Provides services for building and running SQL queries for generating tabular results.

These are used for reports, charts, and security access rules, among other features.

Queries are stored in the entity model, starting at an instance of 'Report'.
They get convered to a StructuredQuery, which may be then compiled to SQL, then executed.
