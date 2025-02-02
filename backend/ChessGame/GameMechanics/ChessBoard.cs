﻿using System.Text;
using ChessGame.GameMechanics;
using ChessGame.Pieces;

namespace ChessGame
{
    public class ChessBoard
    {
        public Piece[,] board;
        public int row = 8;
        public int column = 8;
        public List<Piece> WhiteCaptured;
        public List<Piece> BlackCaptured;
        public int noCaptureCounter = 0;
        public bool isWhiteTimerOver = false;
        public bool isBlackTimerOver = false;
        
        public List<Move> whiteMoves = new List<Move>();
        public List<Move> blackMoves = new List<Move>();
        public int fiftyMoveRuleCounter = 0;
        Dictionary<string, int> positionHistory = new Dictionary<string, int>();

        public Position LastDoubleStepPawn { get; set; }

        public Color ActiveColor { get; set; } = Color.White;
        public string CastlingRights { get; set; } = "KQkq";
        public string EnPassantTarget { get; set; } = "-";
        public int HalfmoveClock { get; set; } = 0;
        public int FullmoveNumber { get; set; } = 1;
        public string[] modes = ["960", "brain-hand", "The king is dead, long live the king!"];
        string mode;
        public ChessBoard(string mode="")
        {
            board = new Piece[column, row];
            WhiteCaptured = new List<Piece>();
            BlackCaptured = new List<Piece>();
            InitializeBoard();
            LastDoubleStepPawn = new Position(-1, -1);
            this.mode = mode;
        }

        private void InitializeBoard()
        {
            Color[] colors = [Color.White, Color.Black];
            int[] lines = [0, row - 1];

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    board[j, i] = PieceFactory.CreatePiece(PieceType.None, Color.None);
                }
            }


            for (int i = 0; i < column; i++)
            {
                board[i, 1] = PieceFactory.CreatePiece(PieceType.Pawn, Color.White);
                board[i, column - 2] = PieceFactory.CreatePiece(PieceType.Pawn, Color.Black);
            }

            for (int i = 0; i < colors.Length; i++)
            {
                board[0, lines[i]] = PieceFactory.CreatePiece(PieceType.Rook, colors[i]);
                board[column - 1, lines[i]] = PieceFactory.CreatePiece(PieceType.Rook, colors[i]);

                board[1, lines[i]] = PieceFactory.CreatePiece(PieceType.Knight, colors[i]);
                board[column - 2, lines[i]] = PieceFactory.CreatePiece(PieceType.Knight, colors[i]);

                board[2, lines[i]] = PieceFactory.CreatePiece(PieceType.Bishop, colors[i]);
                board[column - 3, lines[i]] = PieceFactory.CreatePiece(PieceType.Bishop, colors[i]);

                board[3, lines[i]] = PieceFactory.CreatePiece(PieceType.Queen, colors[i]);
                board[column - 4, lines[i]] = PieceFactory.CreatePiece(PieceType.King, colors[i]);

            }

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    Piece piece = board[j, i];
                    piece.setPosition(new Position(j, i));
                }
            }

        }

        public void EnPassantUpdate(Position start, Position end)
        {
            Piece piece = GetPieceAt(start);
            if (piece.pieceType == PieceType.Pawn && Math.Abs(start.y - end.y) == 2)
            {
                LastDoubleStepPawn = end;
            }
            else
            {
                LastDoubleStepPawn = new Position(-1, -1);
            }

        }
        public void PrintBoard()
        {
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    Piece p = board[j, i];

                    Console.Write(p.color.ToString()[0]);
                    Console.Write(p.pieceType.ToString()[0]);
                    Console.Write(" ");

                }
                Console.WriteLine();
            }
        }

        public Piece GetPieceAt(Position position)
        {
            if (position.x < 0 || position.x >= column || position.y < 0 || position.y >= row) return PieceFactory.CreatePiece(PieceType.None, Color.None);
            return board[position.x, position.y];
        }

        public Piece GetPieceAt(int x, int y)
        {
            return board[x, y];
        }

        public bool MakeMove(Position start, Position end)
        {
            Piece piece = GetPieceAt(start);
            if (piece.pieceType == PieceType.None) return false;
            if (!piece.IsMovePossible(start, end, this)) return false;

            EnPassantUpdate(start, end);
            var color = piece.color;

            if (color == Color.White) whiteMoves.Add(new Move(start, end));
            if (color == Color.Black) blackMoves.Add(new Move(start, end));

            if (piece.pieceType == PieceType.King && Math.Abs(start.x - end.x) == 2)
            {
                MakeCastleMove(start, end);
                piece.isMoved = true;
                UpdatePostMove(piece, start, end, null);
                UpdateFiftyMoveRule(piece, null);

                return true;
            }

            Piece pieceCaptured = GetPieceAt(end);

            Position adjacentPawnPosition = new Position(end.x, start.y);

            if (piece.pieceType == PieceType.Pawn &&
                GetPieceAt(adjacentPawnPosition).pieceType == PieceType.Pawn &&
                pieceCaptured.pieceType == PieceType.None
                )
            {
                pieceCaptured = GetPieceAt(adjacentPawnPosition);
                board[pieceCaptured.position.x, pieceCaptured.position.y] = PieceFactory.
                    CreatePiece(PieceType.None, Color.None);
            }




            if (pieceCaptured.pieceType != PieceType.None)
            {

                if (pieceCaptured.color == Color.White) WhiteCaptured.Add(pieceCaptured);
                if (pieceCaptured.color == Color.Black) BlackCaptured.Add(pieceCaptured);
            }



            piece.isMoved = true;
            board[end.x, end.y] = piece;
            piece.setPosition(end);
            board[start.x, start.y] = PieceFactory.CreatePiece(PieceType.None, Color.None);
            UpdateFiftyMoveRule(piece, pieceCaptured);

            UpdatePostMove(piece, start, end, pieceCaptured);
            return true;
        }
        private void UpdatePostMove(Piece movedPiece, Position start, Position end, Piece capturedPiece)
        {
            // Aktualizacja HalfmoveClock
            if (movedPiece.pieceType == PieceType.Pawn || (capturedPiece != null && capturedPiece.pieceType != PieceType.None))
                HalfmoveClock = 0;
            else
                HalfmoveClock++;

            UpdateCastlingRights(movedPiece, start);

            // Aktualizacja En Passant
            if (movedPiece.pieceType == PieceType.Pawn && Math.Abs(start.y - end.y) == 2)
            {
                // Ustalanie pola en passant, np. dla białych przechodzących z 2 na 4 rankę
                int epY = (start.y + end.y) / 2;
                EnPassantTarget = $"{(char)('a' + end.x)}{epY + 1}";
            }
            else
            {
                EnPassantTarget = "-";
            }

            // Zmiana aktywnego koloru
            ActiveColor = (ActiveColor == Color.White) ? Color.Black : Color.White;

            // Zwiększenie numeru ruchu po ruchu czarnych
            if (ActiveColor == Color.White)
                FullmoveNumber++;
        }
        private void UpdateCastlingRights(Piece movedPiece, Position start)
        {
            // Uproszczona logika aktualizacji praw roszady po ruchu króla lub wieży
            if (movedPiece.pieceType == PieceType.King)
            {
                if (movedPiece.color == Color.White)
                {
                    CastlingRights = CastlingRights.Replace("K", "").Replace("Q", "");
                }
                else
                {
                    CastlingRights = CastlingRights.Replace("k", "").Replace("q", "");
                }
            }
            else if (movedPiece.pieceType == PieceType.Rook)
            {
                if (movedPiece.color == Color.White)
                {
                    if (start.Equals(new Position(0, 0))) CastlingRights = CastlingRights.Replace("Q", "");
                    if (start.Equals(new Position(7, 0))) CastlingRights = CastlingRights.Replace("K", "");
                }
                else
                {
                    if (start.Equals(new Position(0, 7))) CastlingRights = CastlingRights.Replace("q", "");
                    if (start.Equals(new Position(7, 7))) CastlingRights = CastlingRights.Replace("k", "");
                }
            }
        }

        public void MakeMoveWithoutChecking(Position start, Position end)
        {
            Piece piece = GetPieceAt(start);
            if (piece.pieceType == PieceType.King && Math.Abs(start.x - end.x) == 2)
            {
                MakeCastleMove(start, end);
            }
            Piece pieceCaptured = GetPieceAt(end);
            if (pieceCaptured.color == Color.White) WhiteCaptured.Add(pieceCaptured);
            if (pieceCaptured.color == Color.Black) BlackCaptured.Add(pieceCaptured);

            piece.isMoved = true;
            board[end.x, end.y] = piece;
            piece.setPosition(end);
            board[start.x, start.y] = PieceFactory.CreatePiece(PieceType.None, Color.None);

        }

        public void MakeCastleMove(Position start, Position end)
        {
            int deltaX = start.x - end.x;
            if (deltaX == 2)
            {
                board[end.x, end.y] = board[start.x, start.y];
                board[end.x, end.y].setPosition(end);
                board[start.x, start.y] = PieceFactory.CreatePiece(PieceType.None, Color.None);

                board[end.x + 1, end.y] = board[0, start.y];
                board[end.x + 1, end.y].setPosition(new Position(end.x + 1, end.y));
                board[0, start.y] = PieceFactory.CreatePiece(PieceType.None, Color.None);
            }
            if (deltaX == -2)
            {
                board[end.x, end.y] = board[start.x, start.y];
                board[end.x, end.y].setPosition(end);
                board[start.x, start.y] = PieceFactory.CreatePiece(PieceType.None, Color.None);

                board[end.x - 1, end.y] = board[this.column - 1, start.y];
                board[end.x - 1, end.y].setPosition(new Position(end.x - 1, end.y));
                board[this.column - 1, start.y] = PieceFactory.CreatePiece(PieceType.None, Color.None);
            }
        }

        public bool IsKingInCheck(Color kingColor)
        {
            Position kingPosition = FindKingPosition(kingColor);

            // Find the king's position on the board


            if (kingPosition == null)
            {
                throw new Exception("King not found on the board!");
            }

            // Check if any opposing piece can attack the king's position
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    Piece piece = board[j, i];
                    if (piece.color != kingColor && piece.color != Color.None)
                    {
                        if (piece.IsMovePossible(piece.position, kingPosition, this))
                        {
                            return true; // The king is in check
                        }
                    }
                }
            }

            return false; // The king is not in check
        }



        public Position FindKingPosition(Color color)
        {
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    Piece piece = board[i, j];
                    if (piece.pieceType == PieceType.King && piece.color == color)
                        return new Position(i, j);

                }
            }
            return new Position(-1, -1);
        }


        public bool ifCheckmate(Color color)
        {
            if (!IsKingInCheck(color)) return false;
            Console.Write($"test {color}");
            Position kingPosition = FindKingPosition(color);
            List<Move> allMoves = GetAllPlayerMoves(color);
            Console.WriteLine($"liczba ruchów {color} : {allMoves.Count}");
            Console.WriteLine($"pozycja krola {color} : {kingPosition}");

            foreach (Move move in allMoves)
            {
                ChessBoard boardCopy = CreateChessBoardCopy();


                boardCopy.MakeMove(move.from, move.to);
                if (!(boardCopy.IsKingInCheck(color)))
                {
                    return false;
                }
            }
            return true;

        }



        ////public bool ifCheckmate(Color kingColor)
        ////{
        ////    var moves = GetAllPlayerMoves(kingColor);
        ////    if (!ifCheck(kingColor)) return false;
        ////    foreach (var move in moves)
        ////    {
        ////        ChessBoard boardCopy = CreateChessBoardCopy();
        ////        boardCopy.MakeMove(move.from, move.to);

        ////        if ((boardCopy.ifCheck(kingColor)))
        ////        {
        ////            return false;
        ////        }
        ////    }
        ////    return true;
        ////}
        public List<Piece> GetPlayerPieces(Color color)
        {
            List<Piece> playerPieces = new List<Piece>();
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {

                    Piece piece = board[i, j];
                    if (piece.color == color)
                        playerPieces.Add(piece);
                }
            }
            return playerPieces;
        }

        public List<Move> GetAllPieceMoves(Piece piece)
        {
            List<Move> moves = new List<Move>();

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    if (piece.IsMovePossible(piece.position, new Position(i, j), this))
                        moves.Add(new Move(piece.position, new Position(i, j)));
                }
            }

            return moves;
        }
        public Color GetOppositeColor(Color color)
        {
            return color == Color.White ? Color.Black : Color.White;
        }


        public List<Move> GetAllPieceMoves(Position position)
        {
            List<Move> moves = new List<Move>();

            Piece piece = GetPieceAt(position);
            Color color = piece.color;
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    if (piece.IsMovePossible(piece.position, new Position(i, j), this))
                    {
                        ChessBoard boardCopy = CreateChessBoardCopy();

                        boardCopy.MakeMove(piece.position, new Position(i, j));

                        if (!(boardCopy.IsKingInCheck(GetOppositeColor(color))))
                        {
                            moves.Add(new Move(piece.position, new Position(i, j)));

                        }
                    }
                }
            }

            return moves;
        }
        public List<Move> GetAllPlayerMoves(Color color)
        {
            List<Move> moves = new List<Move>();
            List<Piece> playerPieces = GetPlayerPieces(color);

            foreach (Piece piece in playerPieces)
            {
                for (int i = 0; i < row; i++)
                {
                    for (int j = 0; j < column; j++)
                    {
                        if (piece.IsMovePossible(piece.position, new Position(i, j), this))

                        {
                            if (mode == modes[2])
                            {
                                moves.Add(new Move(piece.position, new Position(i, j)));

                            }
                            else
                            {
                                ChessBoard boardCopy = CreateChessBoardCopy();

                                boardCopy.MakeMove(piece.position, new Position(i, j));
                                if (!(boardCopy.IsKingInCheck(color)))
                                {
                                    moves.Add(new Move(piece.position, new Position(i, j)));

                                }
                            }
                        }

                    }
                }
            }
            return moves;
        }
        public void Promotion(Position position, PieceType pieceType)
        {

            Piece piece = GetPieceAt(position);
            if (piece.pieceType != PieceType.Pawn) return;
            if (piece.color == Color.Black && position.y != 0) return;
            if (piece.color == Color.White && position.y == this.row - 1) return;
            board[position.x, position.y] = PieceFactory.CreatePiece(pieceType, piece.color);
        }

        public Piece[,] CreateBoardCopy()
        {
            Piece[,] boardCopy = new Piece[this.column, this.row];


            for (int i = 0; i < column; i++)
            {
                for (int j = 0; j < row; j++)
                {
                    Piece piece = board[i, j];
                    boardCopy[i, j] = piece.Clone();
                }
            }

            return boardCopy;
        }

        public ChessBoard CreateChessBoardCopy()
        {
            ChessBoard copy = new ChessBoard();

            for (int i = 0; i < column; i++)
            {
                for (int j = 0; j < row; j++)
                {

                    Piece piece = board[i, j];
                    copy.board[i, j] = piece.Clone();
                }
            }


            copy.WhiteCaptured = new List<Piece>(WhiteCaptured);
            copy.BlackCaptured = new List<Piece>(BlackCaptured);

            return copy;
        }
        public Piece this[Position position]
        {
            get
            {
                return board[position.x, position.y];
            }
            set
            {
                board[position.x, position.y] = value;

            }
        }

        void UpdateFiftyMoveRule(Piece pieceMoved, Piece pieceCaptured)
        {
            if (pieceCaptured != null && pieceCaptured.pieceType != PieceType.None || pieceMoved.pieceType == PieceType.Pawn)
            {
                fiftyMoveRuleCounter = 0;
            }
            else
            {
                fiftyMoveRuleCounter++;
            }
        }
        public string GenerateFEN()
        {
            StringBuilder fen = new StringBuilder();
            Console.WriteLine("test fen");
            // Iterate over each row to construct piece positions
            for (int y = row - 1; y >= 0; y--)
            {
                int emptySquares = 0;
                for (int x = 0; x < column; x++)
                {
                    Piece piece = board[x, y];
                    if (piece.pieceType == PieceType.None)
                    {
                        emptySquares++;
                    }
                    else
                    {
                        if (emptySquares > 0)
                        {
                            fen.Append(emptySquares);
                            emptySquares = 0;
                        }
                        char symbol = GetFENPieceSymbol(piece);
                        fen.Append(symbol);
                    }
                }
                if (emptySquares > 0)
                {
                    fen.Append(emptySquares);
                }
                if (y > 0)
                {
                    fen.Append('/');
                }
            }

            // Użycie dynamicznych pól do budowy pozostałej części FEN
            fen.Append(" ").Append(ActiveColor == Color.White ? "w" : "b");
            fen.Append(" ").Append(string.IsNullOrEmpty(CastlingRights) ? "-" : CastlingRights);
            fen.Append(" ").Append(EnPassantTarget);
            fen.Append(" ").Append(HalfmoveClock);
            fen.Append(" ").Append(FullmoveNumber);

            return fen.ToString();
        }

        private char GetFENPieceSymbol(Piece piece)
        {
            // Define symbols for pieces
            char symbol = piece.pieceType switch
            {
                PieceType.Pawn => 'P',
                PieceType.Rook => 'R',
                PieceType.Knight => 'N',
                PieceType.Bishop => 'B',
                PieceType.Queen => 'Q',
                PieceType.King => 'K',
                _ => ' '
            };

            // Use lowercase for black pieces, uppercase for white
            return piece.color == Color.Black ? char.ToLower(symbol) : symbol;
        }
        //to do
        private string GetCastlingRights()
        {
            StringBuilder rights = new StringBuilder();

            // Castling rights based on piece movement
            if (!board[4, 0].isMoved && !board[7, 0].isMoved) rights.Append("K"); // White kingside
            if (!board[4, 0].isMoved && !board[0, 0].isMoved) rights.Append("Q"); // White queenside
            if (!board[4, 7].isMoved && !board[7, 7].isMoved) rights.Append("k"); // Black kingside
            if (!board[4, 7].isMoved && !board[0, 7].isMoved) rights.Append("q"); // Black queenside

            // Return "-" if no castling rights, or the available rights
            return rights.Length > 0 ? rights.ToString() : "-";
        }

        private string GetEnPassantTarget()
        {
            // Placeholder en passant target; replace with actual logic as needed
            return "-";
        }

        public void LoadFEN(string fen)
        {
            Console.WriteLine($"otrzymany fen :{fen}");
            string[] fenParts = fen.Split(' ');
            string boardState = fenParts[0]; // Część notacji FEN opisująca stan planszy

            // Wyczyść planszę
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    board[j, i] = PieceFactory.CreatePiece(PieceType.None, Color.None);
                }
            }

            int currentRow = 7; // Od górnego rzędu
            int currentCol = 0; // Od lewej kolumny

            foreach (char c in boardState)
            {
                if (c == '/') // Przejście do następnego rzędu
                {
                    currentRow--;
                    currentCol = 0;
                }
                else if (char.IsDigit(c)) // Liczba pustych pól
                {
                    currentCol += int.Parse(c.ToString());
                }
                else // Figura
                {
                    Color color = char.IsUpper(c) ? Color.White : Color.Black;
                    PieceType type = CharToPieceType(char.ToLower(c));
                    board[currentCol, currentRow] = PieceFactory.CreatePiece(type, color);
                    board[currentCol, currentRow].setPosition(new Position(currentCol, currentRow));
                    currentCol++;
                }
            }
            PrintBoard();
        }

        private PieceType CharToPieceType(char c)
        {
            return c switch
            {
                'p' => PieceType.Pawn,
                'r' => PieceType.Rook,
                'n' => PieceType.Knight,
                'b' => PieceType.Bishop,
                'q' => PieceType.Queen,
                'k' => PieceType.King,
                _ => PieceType.None
            };
        }
        /// <summary>
        /// Generating algebraic notation for each move  (such as e4, Nxf3+, O-O, ect.)
        /// </summary>
        /// <param name="board">Current board position.</param>
        /// <param name="move"> Move contains information about a move (start, end).</param>
        /// <returns>String with algebraic notation.</returns>
        public string GenerateAlgebraicNotation(ChessBoard board, Move move)
        {
            Piece movedPiece = board.GetPieceAt(move.from);
            if (movedPiece == null || movedPiece.pieceType == PieceType.None)
            {
                throw new ArgumentException($"No such figure {move.from}. Notation connot be generated.");
            }


            char pieceLetter = GetFENPieceSymbol(movedPiece);
                
            pieceLetter = char.ToUpper(pieceLetter);

            // 3. Castle.
            //    Short white castling:]:  e1 -> g1  (start=(4,0), end=(6,0))
            //    Long white castling:   e1 -> c1  (start=(4,0), end=(2,0))
            //    Short black castling:: e8 -> g8  (start=(4,7), end=(6,7))
            //    Long black castling  e8 -> c8  (start=(4,7), end=(2,7))
            if (movedPiece.pieceType == PieceType.King && Math.Abs(move.from.x - move.to.x) == 2)
            {
                //Short castling
                if (move.to.x == 6) return "O-O";
                // Long castling
                if (move.to.x == 2) return "O-O-O";
            }

            // 4. Sprawdź, czy to bicie.  
            //    - Bicie normalne: na polu docelowym stoi figura przeciwnego koloru.
            //    - Bicie en passant: pionek przesunął się po skosie, ale pole docelowe jest puste.
            //      W Twojej metodzie `MakeMove` sprawdzasz en passant, tutaj wystarczy analogicznie.
            bool isCapture = false;
            Piece targetPiece = board.GetPieceAt(move.to);

            // a) Normalne bicie – na polu docelowym stoi figura innego koloru.
            if (targetPiece != null && targetPiece.pieceType != PieceType.None && targetPiece.color != movedPiece.color)
            {
                isCapture = true;
            }
            // b) En passant – pionek zbija na pusto, ale inne warunki w MakeMove go dopuszczają.
            //    Tutaj uproszczona wersja: pionek zmienia kolumnę (from.x != to.x), ale docelowe pole jest puste.
            else if (movedPiece.pieceType == PieceType.Pawn
                     && move.from.x != move.to.x
                     && (targetPiece == null || targetPiece.pieceType == PieceType.None))
            {
                isCapture = true;
            }

            // 5. W przypadku pionka bijącego (np. exd5), w notacji pionek jest oznaczany literą kolumny początkowej.
            //    Kolumnę (file) ustalamy jako (char)('a' + from.x), a rząd (rank) to from.y + 1, itd.
            //    Dlatego pieceLetter = "" (bo pion nie ma standardowo litery), ale musimy dodać literę kolumny od której rusza.
            string captureSymbol = isCapture ? "x" : "";
            string notationPrefix;
            if (pieceLetter == 'P' || pieceLetter == 'p')
            {
                notationPrefix = "";
            }
            else
            {
                notationPrefix = pieceLetter.ToString();
            }

            if (movedPiece.pieceType == PieceType.Pawn && isCapture)
            {
                // Zamiast pustego pieceLetter dajemy kolumnę początkową, np. 'e'
                notationPrefix = ((char)('a' + move.from.x)).ToString();
            }

            // 6. Utwórz kopię planszy i wykonaj ruch, aby sprawdzić czy nastąpił szach lub mat po tym ruchu.
            ChessBoard boardCopy = board.CreateChessBoardCopy();
            boardCopy.MakeMoveWithoutChecking(move.from, move.to);

            Color movingPieceColor = movedPiece.color;
            Color opponentColor = board.GetOppositeColor(movingPieceColor);

            bool isCheck = boardCopy.IsKingInCheck(opponentColor);
            bool isMate = boardCopy.ifCheckmate(opponentColor);

            string checkOrMateSuffix = isMate ? "#" : (isCheck ? "+" : "");

            // 7. Konwertuj pozycję docelową (move.to) do notacji algebraicznej, np. (0,0) => "a1", (4,7) => "e8".
            string toAlgebraic = PositionToAlgebraic(move.to);

            string algebraic = $"{notationPrefix}{captureSymbol}{toAlgebraic}{checkOrMateSuffix}";

            return algebraic;
        }
        /// <summary>
        /// Zamienia współrzędne (x,y) na zapis algebraiczny pola, np. (0,0) => a1, (7,7) => h8.
        /// W Twojej tablicy (x,y) = (kolumna, rząd).
        /// </summary>
        private string PositionToAlgebraic(Position pos)
        {
            // x => kolumna, 0 = a, 1 = b, ..., 7 = h
            // y => rząd,    0 = 1, 1 = 2, ..., 7 = 8
            char file = (char)('a' + pos.x);
            int rank = pos.y + 1;
            return $"{file}{rank}";
        }


    }
}