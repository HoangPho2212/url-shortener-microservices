export interface UrlRecord {
  id: string;
  originalUrl: string;
  shortUrl: string;
  fullShortUrl: string;
  clicks: number;
  createdAt: string;
}

export interface ShortenRequest {
  originalUrl: string;
}

export interface ShortenResponse {
  shortCode: string;
  shortUrl: string;
}
