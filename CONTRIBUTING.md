# Contributing Guidelines

The key words "**must**", "**must not**", "**should**", "**should not**" and "**may**" in this document are to be interpreted as described in [RFC 2119](http://www.ietf.org/rfc/rfc2119.txt).

## Code of Conduct

This project is adopting the [Contributor Covenant](https://www.contributor-covenant.org/version/2/1/code_of_conduct/) as its Code of Conduct.


## Opening Issues

Whenever you find a bug or think of a new feature, you **may** open an issue on the [issue tracker](https://github.com/OpenHarmony-NET/OpenHarmony.Avalonia) **if they are not already reported**.

### Reporting Bugs

When reporting a bug, you **should** include the following information:

- Version or commit hash of the library
- A minimum (non) working example
- Steps to reproduce the bug
- Expected behavior
- Actual behavior
- Screenshots or logs (if applicable)

And you **must** also provide a descriptive title and you **should** provide the relevant informations as possible.

### Requesting Features

You **should** ensure that the feature you are requesting is not already implemented or requested.

You **must** provide its use case and a clear description of the feature.

You **should** provide code snippets or screenshots to illustrate your request.

## Sending Pull Requests

To be accepted, your code contribution **must** following the current `editorconfig` rules and the code style of the project.

You **should** writting unit tests for your code, if applicable. If you are meeting difficulties on writing unit tests, we **may** help you.

You **should** create a new branch from the `main` branch and name it according to the feature or bug you are working on.

And your commit history **should** be clean and well formatted:

- We are using [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) to structure our commit messages.
    Template:
    ```
    <type>[optional scope]: <description>

    [optional body]

    [optional footer(s)]
    ```
    [For more information](https://www.conventionalcommits.org/en/v1.0.0/#specification)


## Conversions

- We are usiing [Semantic Versioning 2.0](https://semver.org/spec/v2.0.0.html) to version our library.
