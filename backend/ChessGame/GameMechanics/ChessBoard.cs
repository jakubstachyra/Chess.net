using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
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
        public ChessBoard()
        {
            board = new Piece[column, row];
            WhiteCaptured = new List<Piece>();
            BlackCaptured = new List<Piece>();
            InitializeBoard();

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

            if(piece.pieceType==PieceType.King && Math.Abs(start.x-end.x)==2)
            {
                MakeCastleMove(start,end);
                return true;
            }


            Piece pieceCaptured = GetPieceAt(end);
            if (pieceCaptured.color == Color.White) WhiteCaptured.Add(pieceCaptured);
            if (pieceCaptured.color == Color.Black) BlackCaptured.Add(pieceCaptured);


            piece.isMoved = true;
            board[end.x, end.y] = piece;
            piece.setPosition(end);
            board[start.x, start.y] = PieceFactory.CreatePiece(PieceType.None, Color.None);
            return true;
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
            if(deltaX==2)
            {
                board[end.x, end.y] = board[start.x, start.y];
                board[end.x, end.y].setPosition(end);
                board[start.x, start.y] = PieceFactory.CreatePiece(PieceType.None, Color.None);

                board[end.x+1,end.y] = board[0, start.y];
                board[end.x+1, end.y].setPosition(new Position(end.x+1,end.y));
                board[0, start.y] = PieceFactory.CreatePiece(PieceType.None, Color.None);
            }
            if(deltaX==-2)
            {
                board[end.x, end.y] = board[start.x, start.y];
                board[end.x, end.y].setPosition(end);
                board[start.x, start.y] = PieceFactory.CreatePiece(PieceType.None, Color.None);

                board[end.x - 1, end.y] = board[this.column-1, start.y];
                board[end.x - 1, end.y].setPosition(new Position(end.x - 1, end.y));
                board[this.column-1, start.y] = PieceFactory.CreatePiece(PieceType.None, Color.None);
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
                        if (piece.IsMovePossible(piece.position,kingPosition, this))
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
                if(!(boardCopy.IsKingInCheck(color)))
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

                        boardCopy.MakeMove(piece.position, new Position(i,j));

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

            foreach(Piece piece in playerPieces)
            {
                for (int i = 0; i < row; i++)
                {
                    for (int j = 0; j < column; j++)
                    {
                        if (piece.IsMovePossible(piece.position, new Position(i, j), this))

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
            return moves;
        }

        public void Promotion(Position position, PieceType pieceType)
        {

            Piece piece = GetPieceAt(position);
            if (piece.pieceType != PieceType.Pawn) return;
            if (piece.color == Color.Black && position.y != 0) return;
            if (piece.color == Color.White && position.y == this.row - 1) return;
            board[position.x,position.y]=PieceFactory.CreatePiece(pieceType,piece.color);
        }

        public Piece[,] CreateBoardCopy()
        {
           Piece[,] boardCopy = new Piece[this.column,this.row];


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



        public string GenerateFEN()
        {
            StringBuilder fen = new StringBuilder();
            Console.WriteLine("test fen");
            // Iterate over each row to construct piece positions
            for (int y = row-1; y >=0; y--)
            {
                int emptySquares = 0;

                for (int x = 0; x<column; x++)
                {
                    Piece piece = board[x, y];
                    if (piece.pieceType == PieceType.None)
                    {
                        // Count consecutive empty squares
                        emptySquares++;
                    }
                    else
                    {
                        // Append number of empty squares if any
                        if (emptySquares > 0)
                        {
                            fen.Append(emptySquares);
                            emptySquares = 0;
                        }

                        // Get symbol for the piece and add it to FEN string
                        char symbol = GetFENPieceSymbol(piece);
                        fen.Append(symbol);
                    }
                }

                // Append any remaining empty squares at the end of the row
                if (emptySquares > 0)
                {
                    fen.Append(emptySquares);
                }

                // Add '/' to separate rows, except for the last row
                if (y >0)
                {
                    fen.Append('/');
                }
            }

            // Add active color (assuming white to move initially)
            fen.Append(" ").Append("w"); // Replace "w" with "b" if it's black's turn

            // Castling rights
            string castlingRights = GetCastlingRights();
            fen.Append(" ").Append(string.IsNullOrEmpty(castlingRights) ? "-" : castlingRights);

            // En passant target square
            fen.Append(" ").Append(GetEnPassantTarget());

            // Half-move clock (placeholder value, assuming no captures or pawn moves)
            fen.Append(" ").Append(noCaptureCounter);

            // Full-move number (increment as needed in actual game logic)
            fen.Append(" ").Append(1);

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


    }
}