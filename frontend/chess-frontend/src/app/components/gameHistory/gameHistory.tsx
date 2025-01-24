// src/app/components/gameHistory/gameHistory.tsx

import React, { useState, useEffect, useRef, useCallback } from "react";
import axios from "axios";
import BackgroundUI from "../backgroundUI/pages";
import { useAppSelector } from "../../store/hooks"; // Użyj typowanego hooka
import ChessboardComponent from "../chessBoard/chessBoard";
import { useRouter } from "next/navigation";
import "./gameHistory.css";
import { Game, ApiGame } from "../../../types/types";

const GameHistory: React.FC = () => {
  const [games, setGames] = useState<Game[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [initialLoading, setInitialLoading] = useState<boolean>(true);
  const [shouldFetchNext, setShouldFetchNext] = useState<boolean>(false);
  const [offset, setOffset] = useState<number>(0);
  const [hasMore, setHasMore] = useState<boolean>(true);

  // Ekstrakcja danych użytkownika z Reduxa
  const currentUser = useAppSelector((state) => state.user.user);
  const userId = currentUser ? currentUser.userID : null;
  const username = currentUser ? currentUser.username : "";

  const limit = 3;
  const listContainerRef = useRef<HTMLDivElement | null>(null);
  const counterRef = useRef<number>(0);
  const router = useRouter();

  // Definicja funkcji fetchGames z użyciem useCallback
  const fetchGames = useCallback(async (userID: string) => {
    try {
      setInitialLoading(true);
      const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
      const firstLimit = 6;

      const response = await axios.get<ApiGame[]>(
        `${API_BASE_URL}/history/users/${userID}/games?limit=${firstLimit}&offset=0&detailed=false`
      );

      if (response.data.length < firstLimit) {
        setHasMore(false);
      }

      const mappedGames: Game[] = response.data.map((game) => ({
        gameId: game.gameId,
        lastFen: game.lastFen,
        result: game.result,
        whitePlayer: game.whitePlayer,
        blackPlayer: game.blackPlayer,
      }));

      setOffset((prevOffset) => prevOffset + firstLimit);
      setGames((prevGames) => [...prevGames, ...mappedGames]);
    } catch (error: unknown) {
      if (axios.isAxiosError(error)) {
        console.error("Error fetching game history:", error.response?.data || error.message);
      } else {
        console.error("Error fetching game history:", error);
      }
    } finally {
      setInitialLoading(false);
    }
  }, []);

  // Definicja funkcji fetchMoreGames z użyciem useCallback
  const fetchMoreGames = useCallback(async (playerId: string) => {
    try {
      setLoading(true);
      const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

      const response = await axios.get<ApiGame[]>(
        `${API_BASE_URL}/games/${playerId}?limit=${limit}&offset=${offset}&detailed=false`
      );

      if (response.data.length < limit) {
        setHasMore(false);
      }

      const mappedGames: Game[] = response.data.map((game) => ({
        gameId: game.gameId || game.gameId,
        lastFen: game.lastFen,
        result: game.result,
        whitePlayer: game.whitePlayer,
        blackPlayer: game.blackPlayer,
      }));

      if (response.data.length > 0) {
        setGames((prevGames) => [...prevGames, ...mappedGames]);
        setOffset((prevOffset) => prevOffset + limit);
      }
    } catch (error: unknown) {
      if (axios.isAxiosError(error)) {
        console.error("Error fetching more game history:", error.response?.data || error.message);
      } else {
        console.error("Error fetching more game history:", error);
      }
    } finally {
      setLoading(false);
      setShouldFetchNext(false);
    }
  }, [limit, offset]);

  useEffect(() => {
    if (userId && counterRef.current === 0) {
      fetchGames(userId);
      counterRef.current++;
    }
  }, [userId, fetchGames]);

  useEffect(() => {
    if (userId && shouldFetchNext) {
      fetchMoreGames(userId);
    }
  }, [shouldFetchNext, userId, fetchMoreGames]);

  const handleGameClick = (gameId: string) => {
    router.push(`/history/game-details?gameId=${gameId}`);
  };

  const handleScroll = () => {
    if (!listContainerRef.current || !hasMore) return;

    const { scrollTop, scrollHeight, clientHeight } = listContainerRef.current;
    if (scrollTop + clientHeight >= scrollHeight - 5) {
      if (!shouldFetchNext && !loading) {
        setShouldFetchNext(true);
      }
    }
  };

  const displayedGames = hasMore
    ? [...games, ...Array(limit - (games.length % limit)).fill(null)]
    : games;

  return (
    <BackgroundUI>
      <h1 style={{ color: "white", fontSize: "18px", marginBottom: "5px" }}>Game history</h1>
      {initialLoading ? (
        <div className="loading-spinner bounce-spinner">
          <div className="spinner"></div>
        </div>
      ) : (
        <div className="game-history">
          <div
            className={`games-list-container ${!hasMore ? "no-scroll" : ""}`}
            ref={listContainerRef}
            onScroll={handleScroll}
          >
            <div className="games-list">
              {displayedGames.map((game, index) => (
                <div
                  key={index}
                  className="game-item"
                  onClick={() => game && handleGameClick(game.gameId)}
                  style={{ cursor: game ? "pointer" : "default" }}
                >
                  {game ? (
                    <>
                      <div style={{ cursor: "pointer" }}>
                        <ChessboardComponent
                          position={game.lastFen}
                          isDraggablePiece={() => false}
                          onSquareClick={() => false}
                          boardWidth={200}
                        />
                      </div>
                      <p
                        className={`game-result ${
                          game.result === username
                            ? "win"
                            : game.result === "Draw"
                            ? "draw"
                            : "loss"
                        }`}
                      >
                        {game.result === username
                          ? "You Won"
                          : game.result === "Draw"
                          ? "Draw"
                          : "You Lost"}
                      </p>
                    </>
                  ) : (
                    <div className="empty-board"></div>
                  )}
                </div>
              ))}
            </div>

            {!hasMore && !loading && (
              <div className="no-more-games">
                <p>That&apos;s it!</p> {/* Poprawione escapowanie apostrofu */}
              </div>
            )}
          </div>
          {loading && !initialLoading && (
            <div className="loading-spinner bounce-spinner">
              <div className="spinner"></div>
            </div>
          )}
        </div>
      )}
    </BackgroundUI>
  );
};

export default GameHistory;
