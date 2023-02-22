#include "Arduino.h"

template <class T>
inline Print &operator<<(Print &str, T arg);

//struct LinesAndPositions;

struct LinesAndPositions FindLinesAndPositions(char *filename);
// returns a LineAndPositions struct

void CopyFiles(char *ToFile, char *FromFile);
// copys all data from one file to the other

void ShowFileContents(char *filename);
// shows all the content of the file in serial output

void DeleteSelectedLinesFromFile(char *filename, char *StrOfLines);
// deletes a selected string from a file?

void DeleteLineFromFile(char *filename, int Line);
// delete one selected line from the file

void DeleteMultipleLinesFromFile(char *filename, int SLine, int ELine);
// deletes everything from "a" line - to "b" line