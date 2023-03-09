package rpc

import (
	"context"
	"database/sql"
	"encoding/json"

	"github.com/heroiclabs/nakama-common/runtime"
	"google.golang.org/grpc/codes"
)

type HealthCheckResponse struct {
	Success bool `json:"success"`
}

func RpcHealthCheck(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	logger.Debug("Healthcheck RPC called")
	response := &HealthCheckResponse{Success: true}

	out, err := json.Marshal(response)
	if err != nil {
		logger.Error("Error marshalling response type to JSON: %v", err)
		return "", runtime.NewError("Cannot marshal type", int(codes.Unavailable))
	}

	return string(out), nil
}
