ReadiNow.Expressions
~~~~~~~~~~~~~~~~~~~~

Provides services for compiling and evaluating user-configurable calculation scripts.

These are used by reports, workflows, and document templates, among other features.

Calculations are defined as strings. They are compiled to IExpression.
From there, they can be evaluated, or converted into a StructuredQuery format for inclusion in SQL reports.

ReadiNow.Expression is divided into the following sections:

 * Compiler			-   services to convert scripts into IExpressions
 * Parser			-   internal implementation for actual parsing of scripts
 * Tree				-   internal implementation of the various language elements
 * CalculatedFields -   services to support calculated fields
 * NameResolution   -	services to support working out what field/relationship/type an identifier string is referring to

