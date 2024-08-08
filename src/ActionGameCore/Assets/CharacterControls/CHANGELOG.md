# Changelog

## [0.10.1] - DEVELOP
### Changed
- Changed walk on stairs to smooth.
- changed walk on narrow bridge to smooth.
- Changed input setup for modules.
- Changed move control in air to more fast.
- Changed wall check for pull up movement.
- Changed to directly play the pull up animator state.
### Fixed
- Fixed spring and damper issues.
- Fixed issue where the height was too high when jumping before landing on the ground.

## [0.10.0] - 2024-08-04
### Changed
- The character's movement process has been subdivided into the Module class.
### Added
- Added Pull up module.
- Added character animator controller for each modules. 

## [0.9.0] - 2024-07-07
### Changed
- Changed to use PlayerInput component

## [0.6.1] - 2024-03-06
### Fixed
- Fix input action enabled.

## [0.6.0] - 2024-03-06
### Changed
- Change parameter type of input to InputActionReference.

## [0.5.4] - 2024-03-04
### Fixed
- Fix error with default input action asset.

## [0.5.3] - 2024-03-03
### Fixed
- Clear jump input flag every frame.

## [0.5.2] - 2024-03-03
### Added
- Jump and air walk.

## [0.5.1] - 2024-03-03
### Added
- Add horizontal velocity by floor movement.
### Fixed
- Fix leg suspension for elevator.


## [0.5.0] - 2024-02-27
### Added
- Add character stay and walk.
