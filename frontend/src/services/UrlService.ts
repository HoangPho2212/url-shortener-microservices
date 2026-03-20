import api from './api';
import { UrlRecord, ShortenResponse } from '../contracts/UrlContract';

const UrlService = {
  async getMyUrls(): Promise<UrlRecord[]> {
    const response = await api.get<UrlRecord[]>('/urls/my');
    return response.data;
  },

  async shortenUrl(originalUrl: string): Promise<ShortenResponse> {
    const response = await api.post<ShortenResponse>('/urls/shorten', { originalUrl });
    return response.data;
  }
};

export default UrlService;
