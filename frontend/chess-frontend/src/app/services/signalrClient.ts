// import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

// export const connectToHub = async (url, onHandlers) => {
//   const connection = new HubConnectionBuilder()
//     .withUrl(url)
//     .configureLogging(LogLevel.Information)
//     .withAutomaticReconnect()
//     .build();

//   Object.keys(onHandlers).forEach((event) => {
//     connection.on(event, onHandlers[event]);
//   });

//   await connection.start();
//   return connection;
// };
// services/signalRConnection.ts
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

let connection: HubConnection | null = null;

/**
 * Zwraca (lub tworzy przy pierwszym wywołaniu) globalne połączenie do GameHub.
 * Możesz przekazać obiekt `handlers`, aby zarejestrować obsługę eventów.
 */
export async function getConnection(handlers?: Record<string, (...args: any[]) => void>): Promise<HubConnection> {
  if (!connection) {
    connection = new HubConnectionBuilder()
      .withUrl("https://localhost:7078/gamehub")
      .configureLogging(LogLevel.Information)
      .build();

    // Można uruchomić .start() tutaj lub dopiero po dodaniu handlerów – w zależności od preferencji
    await connection.start();
    console.log("SignalR connection started, ID:", connection.connectionId);
  }

  // Zarejestruj handlery, ale uwaga, żeby nie rejestrować tych samych eventów wiele razy
  if (handlers) {
    Object.entries(handlers).forEach(([eventName, handlerFn]) => {
      // Najpierw warto usunąć poprzedni (jeśli był)
      connection?.off(eventName);
      // Następnie rejestrujemy nowy
      connection?.on(eventName, handlerFn);
    });
  }

  return connection;
}
